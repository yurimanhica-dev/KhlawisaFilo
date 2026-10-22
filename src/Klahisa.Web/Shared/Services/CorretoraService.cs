namespace Klahisa.Web.Shared.Services;

public class CorretoraService
{
    // ---------------- Clientes ----------------
    public Result<Cliente> RegistarCliente(Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Nome)) return Result<Cliente>.Failure("Informe o nome do cliente.");
        if (string.IsNullOrWhiteSpace(c.Telefone)) return Result<Cliente>.Failure("Informe o telefone.");
        if (!string.IsNullOrWhiteSpace(c.Nuit) && M.Clientes.Any(x => x.Nuit == c.Nuit))
            return Result<Cliente>.Failure("Já existe um cliente com este NUIT.");

        c.Id = M.Prox(M.Clientes.Select(x => x.Id));
        c.Codigo = $"CL{c.Id:D4}";
        c.DataRegisto = DateTime.Today;
        M.Clientes.Add(c);
        return Result<Cliente>.Success(c);
    }

    // ---------------- Seguros ----------------
    public Result<Apolice> CriarPedido(Apolice a, int meses)
    {
        if (M.Cli(a.ClienteId) is null) return Result<Apolice>.Failure("Seleccione o cliente.");
        if (M.Seg(a.SeguradoraId) is null) return Result<Apolice>.Failure("Seleccione a seguradora.");
        if (a.Premio <= 0) return Result<Apolice>.Failure("O prémio deve ser superior a zero.");
        if (meses <= 0) meses = 12;

        a.Id = M.Prox(M.Apolices.Select(x => x.Id));
        var cod = (M.Cadastros.FirstOrDefault(c => c.Tipo == "ramos" && c.Descricao == a.Ramo)?.Codigo ?? "GER");
        a.Numero = $"AP/{cod}/{DateTime.Today.Year}/{a.Id:D4}";
        a.Referencia = M.Sequencia("PS", a.Id);
        if (a.Fim == default) a.Fim = a.Inicio.AddMonths(meses).AddDays(-1);
        if (a.DataRenovacao == default) a.DataRenovacao = a.Fim;
        a.Estado = "Pendente";
        M.Apolices.Add(a);

        var id = M.Prox(M.Debitos.Select(x => x.Id));
        M.Debitos.Add(new Debito
        {
            Id = id, Numero = M.Sequencia("DB", id), ApoliceId = a.Id, ClienteId = a.ClienteId, SeguradoraId = a.SeguradoraId,
            Tipo = "Prémio", Descricao = $"Prémio {a.Ramo} — {a.Objecto}", Valor = a.Premio,
            Emissao = a.DataEmissao, Vencimento = a.DataEmissao.AddDays(30), Emitido = false
        });
        return Result<Apolice>.Success(a);
    }

    public Result<Debito> AlocarDebito(int debitoId)
    {
        var d = M.Deb(debitoId);
        if (d is null) return Result<Debito>.Failure("Débito não encontrado.");
        if (d.Emitido) return Result<Debito>.Failure("Este débito já foi emitido.");
        d.Emitido = true;
        d.Emissao = DateTime.Today;
        var ap = M.Apo(d.ApoliceId);
        if (ap is { Estado: "Pendente" }) ap.Estado = "Activa";
        return Result<Debito>.Success(d);
    }

    public Result<Debito> EmitirNota(int apoliceId, string tipo, string motivo, decimal valor)
    {
        var ap = M.Apo(apoliceId);
        if (ap is null) return Result<Debito>.Failure("Seleccione a apólice.");
        if (valor <= 0) return Result<Debito>.Failure("O valor deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(motivo)) return Result<Debito>.Failure("Informe o motivo / descrição.");

        var id = M.Prox(M.Debitos.Select(x => x.Id));
        var d = new Debito
        {
            Id = id, Numero = M.Sequencia(tipo == "Nota de Crédito" ? "NC" : "ND", id), ApoliceId = ap.Id, ClienteId = ap.ClienteId,
            SeguradoraId = ap.SeguradoraId, Tipo = tipo, Descricao = $"{motivo} ({ap.Numero})", Valor = valor,
            Emissao = DateTime.Today, Vencimento = DateTime.Today.AddDays(15), Emitido = true
        };
        M.Debitos.Add(d);
        return Result<Debito>.Success(d);
    }

    // ---------------- Caixa ----------------
    public Result<Pagamento> Pagar(int debitoId, bool completo, decimal valor, string metodo)
    {
        var d = M.Deb(debitoId);
        if (d is null || !d.AReceber) return Result<Pagamento>.Failure("Débito não disponível para pagamento.");
        if (completo) valor = d.Saldo;
        if (valor <= 0) return Result<Pagamento>.Failure("Informe um valor válido.");
        if (valor > d.Saldo) return Result<Pagamento>.Failure($"O valor excede o saldo em dívida ({Fmt.Mt(d.Saldo)}).");
        return Result<Pagamento>.Success(M.RegistarPagamento(d, valor, metodo, DateTime.Today));
    }

    public Result<Pagamento> AnularRecibo(int pagamentoId, string motivo)
    {
        var p = M.Pagamentos.FirstOrDefault(x => x.Id == pagamentoId);
        if (p is null || p.Anulado) return Result<Pagamento>.Failure("Recibo não encontrado ou já anulado.");
        if (string.IsNullOrWhiteSpace(motivo)) return Result<Pagamento>.Failure("Informe o motivo da anulação.");

        p.Anulado = true;
        p.MotivoAnulacao = motivo;
        var d = M.Deb(p.DebitoId);
        if (d is not null) d.Pago -= p.Valor;

        if (p.Contabilizado && p.LancamentoId is int lid)
        {
            var orig = M.Lancamentos.FirstOrDefault(x => x.Id == lid);
            if (orig is not null)
                M.NovoLancamento(DateTime.Today, $"Estorno do recibo {p.Numero}", "Caixa",
                    orig.Linhas.Select(l => (l.Conta, l.Credito, l.Debito)).ToArray());
        }
        return Result<Pagamento>.Success(p);
    }

    public Result<Apolice> AnularApolice(int apoliceId, string motivo)
    {
        var ap = M.Apo(apoliceId);
        if (ap is null || ap.Estado == "Anulada") return Result<Apolice>.Failure("Apólice não encontrada ou já anulada.");
        if (string.IsNullOrWhiteSpace(motivo)) return Result<Apolice>.Failure("Informe o motivo da anulação.");

        var temRecibos = M.Pagamentos.Any(p => !p.Anulado && M.Deb(p.DebitoId)?.ApoliceId == apoliceId);
        if (temRecibos) return Result<Apolice>.Failure("A apólice tem recibos activos. Anule primeiro os recibos.");

        ap.Estado = "Anulada";
        ap.DataAnulacao = DateTime.Today;
        ap.MotivoAnulacao = motivo;
        foreach (var d in M.Debitos.Where(d => d.ApoliceId == apoliceId)) d.Anulado = true;
        return Result<Apolice>.Success(ap);
    }

    public Result<Debito> EditarDebito(int id, string descricao, DateTime vencimento)
    {
        var d = M.Deb(id);
        if (d is null) return Result<Debito>.Failure("Débito não encontrado.");
        if (d.Anulado || d.Pago >= d.Valor) return Result<Debito>.Failure("Débitos pagos ou anulados não podem ser editados.");
        if (string.IsNullOrWhiteSpace(descricao)) return Result<Debito>.Failure("Informe a descrição.");
        d.Descricao = descricao;
        d.Vencimento = vencimento;
        return Result<Debito>.Success(d);
    }

    public Result<int> LancarContabilidade()
    {
        var pendentes = M.Pagamentos.Where(p => !p.Anulado && !p.Contabilizado).ToList();
        if (pendentes.Count == 0) return Result<int>.Failure("Não há pagamentos por lançar.");
        foreach (var p in pendentes) M.ContabilizarPagamento(p);
        return Result<int>.Success(pendentes.Count);
    }

    // ---------------- Seguradoras ----------------
    public (decimal Recebido, decimal Comissao, decimal Liquido, decimal Entregue, decimal Saldo) SaldoSeguradora(int segId)
    {
        var pags = M.Pagamentos.Where(p => !p.Anulado && M.Deb(p.DebitoId)?.SeguradoraId == segId).ToList();
        var recebido = pags.Sum(p => p.Valor);
        var com = pags.Sum(p => M.Comissao(p));
        var entregue = M.Prestacoes.Where(x => x.SeguradoraId == segId).Sum(x => x.Valor);
        return (recebido, com, recebido - com, entregue, recebido - com - entregue);
    }

    public Result<decimal> PrestarContas(int segId)
    {
        var s = SaldoSeguradora(segId);
        if (s.Saldo <= 0) return Result<decimal>.Failure("Não há saldo a entregar a esta seguradora.");
        M.Prestacoes.Add(new PrestacaoContas { Id = M.Prox(M.Prestacoes.Select(x => x.Id)), SeguradoraId = segId, Data = DateTime.Today, Valor = s.Saldo });
        M.NovoLancamento(DateTime.Today, $"Prestação de contas — {M.NomeSeg(segId)}", "Prestação de Contas", ("22", s.Saldo, 0), ("12", 0, s.Saldo));
        return Result<decimal>.Success(s.Saldo);
    }

    public Result<Seguradora> GuardarSeguradora(Seguradora s)
    {
        if (string.IsNullOrWhiteSpace(s.Nome) || string.IsNullOrWhiteSpace(s.Sigla)) return Result<Seguradora>.Failure("Informe o nome e a sigla.");
        s.Id = M.Prox(M.Seguradoras.Select(x => x.Id));
        M.Seguradoras.Add(s);
        return Result<Seguradora>.Success(s);
    }

    // ---------------- Sinistros ----------------
    public Result<Sinistro> RegistarSinistro(Sinistro s)
    {
        if (M.Apo(s.ApoliceId) is null) return Result<Sinistro>.Failure("Seleccione a apólice.");
        if (string.IsNullOrWhiteSpace(s.Descricao)) return Result<Sinistro>.Failure("Descreva a ocorrência.");
        s.Id = M.Prox(M.Sinistros.Select(x => x.Id));
        s.Numero = M.Sequencia("SN", s.Id);
        s.Estado = "Em análise";
        M.Sinistros.Add(s);
        return Result<Sinistro>.Success(s);
    }

    // ---------------- Contabilidade ----------------
    public Result<ContaPlano> GuardarConta(ContaPlano c, bool nova)
    {
        if (string.IsNullOrWhiteSpace(c.Codigo) || string.IsNullOrWhiteSpace(c.Nome)) return Result<ContaPlano>.Failure("Informe o código e o nome.");
        if (nova)
        {
            if (M.Contas.Any(x => x.Codigo == c.Codigo)) return Result<ContaPlano>.Failure("Já existe uma conta com este código.");
            c.Id = M.Prox(M.Contas.Select(x => x.Id));
            M.Contas.Add(c);
            return Result<ContaPlano>.Success(c);
        }
        var ex = M.Contas.FirstOrDefault(x => x.Id == c.Id);
        if (ex is null) return Result<ContaPlano>.Failure("Conta não encontrada.");
        ex.Nome = c.Nome; ex.Tipo = c.Tipo; ex.AceitaLancamentos = c.AceitaLancamentos;
        return Result<ContaPlano>.Success(ex);
    }

    public Result<Lancamento> Lancar(DateTime data, string descricao, string moeda, decimal cambio, List<(string Conta, decimal D, decimal C)> linhas)
    {
        if (string.IsNullOrWhiteSpace(descricao)) return Result<Lancamento>.Failure("Informe a descrição.");
        if (cambio <= 0) cambio = 1;
        var ls = linhas.Where(l => !string.IsNullOrEmpty(l.Conta) && (l.D != 0 || l.C != 0))
            .Select(l => (l.Conta, D: Math.Round(l.D * cambio, 2), C: Math.Round(l.C * cambio, 2))).ToList();
        if (ls.Count < 2) return Result<Lancamento>.Failure("Um lançamento precisa de pelo menos duas linhas.");
        if (ls.Any(l => l.D != 0 && l.C != 0)) return Result<Lancamento>.Failure("Cada linha deve ter débito OU crédito.");
        if (ls.Sum(l => l.D) != ls.Sum(l => l.C)) return Result<Lancamento>.Failure("O lançamento não está equilibrado (débito ≠ crédito).");

        var desc = moeda == "MZN" ? descricao : $"{descricao} ({moeda} @ {cambio:0.00})";
        var lanc = M.NovoLancamento(data, desc, moeda == "MZN" ? "Manual" : "Moeda Estrangeira", ls.ToArray());
        lanc.Moeda = moeda;
        lanc.Cambio = cambio;
        return Result<Lancamento>.Success(lanc);
    }

    public (decimal Debito, decimal Credito) MovimentosConta(string codigo)
    {
        var ls = M.Lancamentos.SelectMany(l => l.Linhas).Where(l => l.Conta == codigo).ToList();
        return (ls.Sum(l => l.Debito), ls.Sum(l => l.Credito));
    }

    public Result<int> ApurarResultados()
    {
        var linhas = new List<(string Conta, decimal D, decimal C)>();
        decimal total = 0;
        foreach (var c in M.Contas.Where(c => c.Tipo is "Proveito" or "Custo"))
        {
            var (d, cr) = MovimentosConta(c.Codigo);
            var saldo = d - cr;
            if (saldo == 0) continue;
            linhas.Add(saldo > 0 ? (c.Codigo, 0m, saldo) : (c.Codigo, -saldo, 0m));
            total += saldo;
        }
        if (linhas.Count == 0) return Result<int>.Failure("Sem saldos de proveitos/custos a apurar.");
        linhas.Add(total > 0 ? ("81", total, 0m) : ("81", 0m, -total));
        M.NovoLancamento(DateTime.Today, "Apuramento de resultados do exercício", "Apuramento", linhas.ToArray());
        return Result<int>.Success(linhas.Count);
    }

    // ---------------- Cadastros genéricos ----------------
    public Result<CadastroItem> GuardarCadastro(CadastroItem i)
    {
        if (string.IsNullOrWhiteSpace(i.Codigo) || string.IsNullOrWhiteSpace(i.Descricao)) return Result<CadastroItem>.Failure("Informe o código e a descrição.");
        if (M.Cadastros.Any(x => x.Tipo == i.Tipo && x.Codigo.Equals(i.Codigo, StringComparison.OrdinalIgnoreCase)))
            return Result<CadastroItem>.Failure("Já existe um registo com este código.");
        i.Id = M.Prox(M.Cadastros.Select(x => x.Id));
        M.Cadastros.Add(i);
        return Result<CadastroItem>.Success(i);
    }

    public Result<CadastroItem> ActualizarCadastro(int id, string codigo, string descricao, string extra)
    {
        var item = M.Cadastros.FirstOrDefault(x => x.Id == id);
        if (item is null) return Result<CadastroItem>.Failure("Registo não encontrado.");
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descricao))
            return Result<CadastroItem>.Failure("Informe o código e a descrição.");
        if (M.Cadastros.Any(x => x.Id != id && x.Tipo == item.Tipo && x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase)))
            return Result<CadastroItem>.Failure("Já existe um registo com este código.");
        item.Codigo = codigo;
        item.Descricao = descricao;
        item.Extra = extra;
        return Result<CadastroItem>.Success(item);
    }

    public Result<bool> EliminarCadastro(int id)
    {
        var i = M.Cadastros.FirstOrDefault(x => x.Id == id);
        if (i is null) return Result<bool>.Failure("Registo não encontrado.");
        M.Cadastros.Remove(i);
        return Result<bool>.Success(true);
    }
}
