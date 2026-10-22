namespace Klahisa.Web.Features.Seguros;

public class SegurosController : BaseController
{
    private readonly CorretoraService _svc;
    public SegurosController(CorretoraService svc) => _svc = svc;

    // ======================= Pedido de Seguro =======================
    private static FormVM FormPedido() => new()
    {
        Titulo = "Pedido de Seguro", Subtitulo = "Registar novo pedido / cotação", ActionUrl = "/Seguros/Pedido", VoltarUrl = "/Seguros/Lista",
        Info = "O pedido fica Pendente e gera um débito por emitir. Use «Alocação de Débitos» para emitir e activar a apólice.",
        Campos =
        {
            CampoVM.Sel("ClienteId", "Cliente", Opcoes.Clientes()),
            CampoVM.Sel("SeguradoraId", "Seguradora", Opcoes.Seguradoras()),
            CampoVM.Sel("Ramo", "Ramo", Opcoes.Cad("ramos")),
            CampoVM.Sel("TipoSeguro", "Tipo de seguro", Opcoes.Cad("tipos-seguro")),
            CampoVM.Txt("Objecto", "Objecto seguro (viatura, imóvel, pessoas…)", larg: 12),
            CampoVM.Num("Capital", "Capital seguro (MT)", obrig: false),
            CampoVM.Num("Premio", "Prémio total (MT)"),
            CampoVM.Dat("Inicio", "Início", DateTime.Today),
            CampoVM.Num("Meses", "Duração (meses)", "12")
        }
    };

    [HttpGet]
    public IActionResult Pedido() => Form(FormPedido());

    [HttpGet]
    public IActionResult Renovar(int id)
    {
        var apolice = M.Apolices.FirstOrDefault(a => a.Id == id && a.Estado == "Activa");
        if (apolice is null) return Falha("Apólice activa não encontrada para renovação.", "/Seguros/ARenovar");

        var vm = FormPedido();
        vm.Titulo = "Renovar Seguro";
        vm.Subtitulo = $"Renovação da apólice {apolice.Numero}";
        vm.Info = "Confirme os dados e ajuste o prémio ou a data de início antes de registar a renovação.";
        vm.Botao = "Registar renovação";
        foreach (var campo in vm.Campos)
        {
            campo.Valor = campo.Nome switch
            {
                "ClienteId" => apolice.ClienteId.ToString(),
                "SeguradoraId" => apolice.SeguradoraId.ToString(),
                "Ramo" => apolice.Ramo,
                "TipoSeguro" => apolice.TipoSeguro,
                "Objecto" => apolice.Objecto,
                "Capital" => Fmt.Num(apolice.Capital),
                "Premio" => Fmt.Num(apolice.Premio),
                "Inicio" => apolice.Fim.AddDays(1).ToString("yyyy-MM-dd"),
                "Meses" => "12",
                _ => campo.Valor
            };
        }
        return Form(vm);
    }

    [HttpPost]
    public IActionResult Pedido(IFormCollection f)
    {
        var a = new Apolice
        {
            ClienteId = Int(f["ClienteId"]), SeguradoraId = Int(f["SeguradoraId"]), Ramo = f["Ramo"].ToString(),
            TipoSeguro = f["TipoSeguro"].ToString(), Objecto = f["Objecto"].ToString(), Capital = Dec(f["Capital"]),
            Premio = Dec(f["Premio"]), Inicio = Dt(f["Inicio"])
        };
        var r = _svc.CriarPedido(a, Int(f["Meses"]));
        if (!r.IsSuccess) return FormComErro(FormPedido(), f, r.Error!);
        return Sucesso($"Pedido {r.Value!.Numero} registado.", "/Seguros/Lista");
    }

    public IActionResult Lista()
    {
        var vm = new TabelaVM
        {
            Titulo = "Listagem de Seguros", Subtitulo = "Apólices e pedidos",
            Colunas = { "Nº Apólice", "Cliente", "Seguradora", "Ramo", "Objecto", "Prémio", "Início", "Fim", "Estado" },
            AcoesTopo = { AcaoVM.Link("Novo pedido", "/Seguros/Pedido", "ti-plus") }
        };
        foreach (var a in M.Apolices.OrderByDescending(a => a.Id))
            vm.Linhas.Add(LinhaVM.De(a.Numero, M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), a.Ramo, a.Objecto,
                Fmt.Mt(a.Premio), Fmt.Dt(a.Inicio), Fmt.Dt(a.Fim), a.Estado));
        return Tabela(vm);
    }

    // ======================= Alocação de Débitos =======================
    public IActionResult AlocacaoDebitos()
    {
        var vm = new TabelaVM
        {
            Titulo = "Alocação de Débitos", Subtitulo = "Débitos por emitir — emitir e activar apólices",
            Colunas = { "Nº Débito", "Apólice", "Cliente", "Seguradora", "Valor", "Vencimento" },
            Vazio = "Não há débitos por emitir."
        };
        foreach (var d in M.Debitos.Where(d => !d.Emitido && !d.Anulado))
            vm.Linhas.Add(LinhaVM.De(d.Numero, M.Apo(d.ApoliceId)?.Numero ?? "—", M.NomeCliente(d.ClienteId), M.NomeSeg(d.SeguradoraId),
                Fmt.Mt(d.Valor), Fmt.Dt(d.Vencimento))
                .Com(AcaoVM.Envio("Emitir / Alocar", $"/Seguros/Alocar/{d.Id}", "Emitir este débito e activar a apólice?", "ti-check")));
        return Tabela(vm);
    }

    [HttpGet]
    public IActionResult PrepararAlocacao(int id)
    {
        var debito = M.Deb(id);
        if (debito is null || debito.Emitido || debito.Anulado)
            return Falha("Débito não disponível para emissão.", "/Seguros/AlocacaoDebitos");

        var vm = new FormVM
        {
            Titulo = "Emitir / Alocar Débito",
            Subtitulo = debito.Numero,
            ActionUrl = $"/Seguros/Alocar/{debito.Id}",
            VoltarUrl = "/Seguros/AlocacaoDebitos",
            Botao = "Emitir e activar apólice",
            Info = "Confirme o débito. A emissão activa a apólice correspondente.",
            Campos =
            {
                CampoVM.Ler("cliente", "Cliente", M.NomeCliente(debito.ClienteId)),
                CampoVM.Ler("apolice", "Apólice", M.Apo(debito.ApoliceId)?.Numero ?? "—"),
                CampoVM.Ler("valor", "Valor", Fmt.Mt(debito.Valor)),
                CampoVM.Ler("vencimento", "Vencimento", Fmt.Dt(debito.Vencimento))
            }
        };
        return Form(vm);
    }

    [HttpPost]
    public IActionResult Alocar(int id)
    {
        var r = _svc.AlocarDebito(id);
        return r.IsSuccess ? Sucesso($"Débito {r.Value!.Numero} emitido. Apólice activada.", "/Seguros/AlocacaoDebitos")
                           : Falha(r.Error!, "/Seguros/AlocacaoDebitos");
    }

    // ======================= Prestação de Contas =======================
    public IActionResult PrestacaoContas()
    {
        var vm = new TabelaVM
        {
            Titulo = "Prestação de Contas", Subtitulo = "Valores a entregar às seguradoras (recebido − comissão)",
            Colunas = { "Seguradora", "Recebido", "Comissão", "Líquido", "Já entregue", "Saldo a entregar" }
        };
        foreach (var s in M.Seguradoras)
        {
            var x = _svc.SaldoSeguradora(s.Id);
            var linha = LinhaVM.De(s.Sigla, Fmt.Mt(x.Recebido), Fmt.Mt(x.Comissao), Fmt.Mt(x.Liquido), Fmt.Mt(x.Entregue), Fmt.Mt(x.Saldo));
            if (x.Saldo > 0)
                linha.Com(AcaoVM.Envio("Prestar contas", $"/Seguros/PrestarContas/{s.Id}", $"Entregar {Fmt.Mt(x.Saldo)} à {s.Sigla}?", "ti-send"));
            vm.Linhas.Add(linha);
        }
        return Tabela(vm);
    }

    [HttpPost]
    public IActionResult PrestarContas(int id)
    {
        var r = _svc.PrestarContas(id);
        return r.IsSuccess ? Sucesso($"Prestação de contas registada: {Fmt.Mt(r.Value)}.", "/Seguros/PrestacaoContas")
                           : Falha(r.Error!, "/Seguros/PrestacaoContas");
    }

    // ======================= Inclusão e Exclusão =======================
    private static FormVM FormIncExc() => new()
    {
        Titulo = "Inclusão e Exclusão", Subtitulo = "Endosso à apólice (inclui/exclui objectos ou pessoas)", ActionUrl = "/Seguros/InclusaoExclusao", VoltarUrl = "/Seguros/Lista",
        Info = "Inclusão gera uma Nota de Débito (prémio adicional). Exclusão gera uma Nota de Crédito (estorno de prémio).",
        Campos =
        {
            CampoVM.Sel("ApoliceId", "Apólice", Opcoes.Apolices(true)),
            CampoVM.Sel("Operacao", "Operação", Opcoes.De("Inclusão", "Exclusão")),
            CampoVM.Txt("Descricao", "Descrição (o que é incluído/excluído)", larg: 12),
            CampoVM.Num("Valor", "Valor do ajuste (MT)")
        }
    };

    [HttpGet]
    public IActionResult InclusaoExclusao() => Form(FormIncExc());

    [HttpPost]
    public IActionResult InclusaoExclusao(IFormCollection f)
    {
        var tipo = f["Operacao"] == "Exclusão" ? "Nota de Crédito" : "Nota de Débito";
        var r = _svc.EmitirNota(Int(f["ApoliceId"]), tipo, $"{f["Operacao"]}: {f["Descricao"]}", Dec(f["Valor"]));
        if (!r.IsSuccess) return FormComErro(FormIncExc(), f, r.Error!);
        return Sucesso($"{tipo} {r.Value!.Numero} emitida.", "/Seguros/Notas");
    }

    // ======================= Nota de Débito e Crédito =======================
    private static FormVM FormNota() => new()
    {
        Titulo = "Nota de Débito e Crédito", Subtitulo = "Emissão manual de notas", ActionUrl = "/Seguros/NotaDebitoCredito", VoltarUrl = "/Seguros/Notas",
        Botao = "Emitir nota",
        Campos =
        {
            CampoVM.Sel("ApoliceId", "Apólice", Opcoes.Apolices(true)),
            CampoVM.Sel("Tipo", "Tipo de nota", Opcoes.De("Nota de Débito", "Nota de Crédito")),
            CampoVM.Txt("Motivo", "Motivo", larg: 12),
            CampoVM.Num("Valor", "Valor (MT)")
        }
    };

    [HttpGet]
    public IActionResult NotaDebitoCredito() => Form(FormNota());

    [HttpPost]
    public IActionResult NotaDebitoCredito(IFormCollection f)
    {
        var r = _svc.EmitirNota(Int(f["ApoliceId"]), f["Tipo"].ToString(), f["Motivo"].ToString(), Dec(f["Valor"]));
        if (!r.IsSuccess) return FormComErro(FormNota(), f, r.Error!);
        return Sucesso($"{r.Value!.Tipo} {r.Value.Numero} emitida.", "/Seguros/Notas");
    }

    public IActionResult Notas()
    {
        var vm = new TabelaVM
        {
            Titulo = "Notas de Débito e Crédito", Subtitulo = "Notas emitidas",
            Colunas = { "Nº Nota", "Tipo", "Apólice", "Cliente", "Descrição", "Valor", "Estado" },
            AcoesTopo = { AcaoVM.Link("Nova nota", "/Seguros/NotaDebitoCredito", "ti-plus") }
        };
        foreach (var d in M.Debitos.Where(d => d.Tipo != "Prémio").OrderByDescending(d => d.Id))
            vm.Linhas.Add(LinhaVM.De(d.Numero, d.Tipo, M.Apo(d.ApoliceId)?.Numero ?? "—", M.NomeCliente(d.ClienteId), d.Descricao, Fmt.Mt(d.Valor), d.Estado));
        return Tabela(vm);
    }

    // ======================= Relatórios de Seguros =======================
    private static TabelaVM TabelaDebitos(string titulo, string sub, IEnumerable<Debito> dados)
    {
        var vm = new TabelaVM { Titulo = titulo, Subtitulo = sub, Colunas = { "Nº Débito", "Cliente", "Seguradora", "Apólice", "Valor", "Pago", "Saldo", "Vencimento", "Estado" } };
        foreach (var d in dados)
            vm.Linhas.Add(LinhaVM.De(d.Numero, M.NomeCliente(d.ClienteId), M.NomeSeg(d.SeguradoraId), M.Apo(d.ApoliceId)?.Numero ?? "—",
                Fmt.Mt(d.Valor), Fmt.Mt(d.Pago), Fmt.Mt(d.Saldo), Fmt.Dt(d.Vencimento), d.Estado));
        vm.Resumo.Add(("Registos", vm.Linhas.Count.ToString()));
        vm.Resumo.Add(("Total em saldo", Fmt.Mt(dados.Sum(d => d.Saldo))));
        return vm;
    }

    public IActionResult ARenovar()
    {
        var h = DateTime.Today;
        var vm = new TabelaVM
        {
            Titulo = "Seguros por Renovar", Subtitulo = "Apólices activas a terminar nos próximos 60 dias",
            Colunas = { "Nº Apólice", "Cliente", "Seguradora", "Ramo", "Prémio", "Fim", "Dias restantes" }
        };
        foreach (var a in M.Apolices.Where(a => a.Estado == "Activa" && a.Fim >= h && a.Fim <= h.AddDays(60)).OrderBy(a => a.Fim))
            vm.Linhas.Add(LinhaVM.De(a.Numero, M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), a.Ramo, Fmt.Mt(a.Premio), Fmt.Dt(a.Fim), (a.Fim - h).Days.ToString()));
        return Tabela(vm);
    }

    public IActionResult DebitosPorVencer() =>
        Tabela(TabelaDebitos("Débitos por Vencer", "Débitos emitidos ainda dentro do prazo", M.Debitos.Where(d => d.AReceber && d.Vencimento.Date >= DateTime.Today).OrderBy(d => d.Vencimento)));

    public IActionResult DebitosVencidos() =>
        Tabela(TabelaDebitos("Débitos Vencidos", "Débitos em atraso", M.Debitos.Where(d => d.AReceber && d.Vencimento.Date < DateTime.Today).OrderBy(d => d.Vencimento)));

    public IActionResult DebitosPagos() =>
        Tabela(TabelaDebitos("Débitos Pagos", "Débitos totalmente liquidados", M.Debitos.Where(d => d.Estado == "Pago").OrderByDescending(d => d.Vencimento)));

    public IActionResult ExtractoCliente(int? clienteId)
    {
        var vm = new TabelaVM
        {
            Titulo = "Extracto do Cliente", Subtitulo = "Movimentos de débito e crédito", Pesquisavel = false, Vazio = "Seleccione um cliente.",
            Colunas = { "Data", "Documento", "Descrição", "Débito", "Crédito", "Saldo" },
            Filtro = new FiltroVM { Nome = "clienteId", Rotulo = "Cliente", Url = "/Seguros/ExtractoCliente", Valor = clienteId?.ToString() ?? "", Opcoes = Opcoes.Clientes() }
        };
        if (clienteId is null) return Tabela(vm);

        var movs = new List<(DateTime Data, string Doc, string Desc, decimal D, decimal C)>();
        foreach (var d in M.Debitos.Where(d => d.ClienteId == clienteId && d.Emitido && !d.Anulado))
            movs.Add(d.Tipo == "Nota de Crédito" ? (d.Emissao, d.Numero, d.Descricao, 0m, d.Valor) : (d.Emissao, d.Numero, d.Descricao, d.Valor, 0m));
        foreach (var p in M.Pagamentos.Where(p => !p.Anulado && M.Deb(p.DebitoId)?.ClienteId == clienteId))
            movs.Add((p.Data, p.Numero, $"Pagamento — {p.Metodo}", 0m, p.Valor));

        decimal saldo = 0;
        foreach (var m in movs.OrderBy(m => m.Data))
        {
            saldo += m.D - m.C;
            vm.Linhas.Add(LinhaVM.De(Fmt.Dt(m.Data), m.Doc, m.Desc, m.D > 0 ? Fmt.Mt(m.D) : "", m.C > 0 ? Fmt.Mt(m.C) : "", Fmt.Mt(saldo)));
        }
        vm.Resumo.Add(("Cliente", M.NomeCliente(clienteId.Value)));
        vm.Resumo.Add(("Saldo em dívida", Fmt.Mt(saldo)));
        return Tabela(vm);
    }

    public IActionResult ListaPagamentos()
    {
        var vm = new TabelaVM
        {
            Titulo = "Lista de Pagamentos", Subtitulo = "Todos os recibos emitidos",
            Colunas = { "Recibo", "Data", "Cliente", "Débito", "Método", "Valor", "Tipo", "Estado" }
        };
        foreach (var p in M.Pagamentos.OrderByDescending(p => p.Data).ThenByDescending(p => p.Id))
        {
            var d = M.Deb(p.DebitoId);
            vm.Linhas.Add(LinhaVM.De(p.Numero, Fmt.Dt(p.Data), M.NomeCliente(d?.ClienteId ?? 0), d?.Numero ?? "—", p.Metodo, Fmt.Mt(p.Valor),
                p.Parcial ? "Parcial" : "Completo", p.Anulado ? "Anulado" : "Pago"));
        }
        vm.Resumo.Add(("Total recebido (válidos)", Fmt.Mt(M.Pagamentos.Where(p => !p.Anulado).Sum(p => p.Valor))));
        return Tabela(vm);
    }

    public IActionResult SaldoSeguradoras()
    {
        var vm = new TabelaVM
        {
            Titulo = "Saldo das Seguradoras", Subtitulo = "Posição por seguradora",
            Colunas = { "Seguradora", "Prémios recebidos", "Comissão", "Líquido", "Entregue", "Saldo" }, Pesquisavel = false
        };
        foreach (var s in M.Seguradoras)
        {
            var x = _svc.SaldoSeguradora(s.Id);
            vm.Linhas.Add(LinhaVM.De(s.Nome, Fmt.Mt(x.Recebido), Fmt.Mt(x.Comissao), Fmt.Mt(x.Liquido), Fmt.Mt(x.Entregue), Fmt.Mt(x.Saldo)));
        }
        return Tabela(vm);
    }
}
