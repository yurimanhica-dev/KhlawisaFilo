namespace Klahisa.Web.Shared.Data;

public class Cliente
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Nuit { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string NumDocumento { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Endereco { get; set; } = "";
    public DateTime DataRegisto { get; set; }
}

public class Seguradora
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public string Nuit { get; set; } = "";
    public string Telefone { get; set; } = "";
    public decimal ComissaoPct { get; set; }
}

public class Apolice
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int ClienteId { get; set; }
    public int SeguradoraId { get; set; }
    public string Ramo { get; set; } = "";
    public string TipoSeguro { get; set; } = "";
    public string Objecto { get; set; } = "";
    public decimal Capital { get; set; }
    public decimal Premio { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string Estado { get; set; } = "Pendente"; // Pendente | Activa | Anulada
    public DateTime? DataAnulacao { get; set; }
    public string? MotivoAnulacao { get; set; }
}

public class Debito
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int ApoliceId { get; set; }
    public int ClienteId { get; set; }
    public int SeguradoraId { get; set; }
    public string Tipo { get; set; } = "Prémio"; // Prémio | Nota de Débito | Nota de Crédito
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Pago { get; set; }
    public DateTime Emissao { get; set; }
    public DateTime Vencimento { get; set; }
    public bool Emitido { get; set; } = true;
    public bool Anulado { get; set; }

    public decimal Saldo => Valor - Pago;

    public bool AReceber => Emitido && !Anulado && Tipo != "Nota de Crédito" && Pago < Valor;

    public string Estado =>
        Tipo == "Nota de Crédito" ? (Anulado ? "Anulado" : "Emitido") :
        Anulado ? "Anulado" :
        !Emitido ? "Por emitir" :
        Pago >= Valor ? "Pago" :
        Vencimento.Date < DateTime.Today ? "Vencido" :
        Pago > 0 ? "Parcial" : "Pendente";
}

public class Pagamento
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int DebitoId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public string Metodo { get; set; } = "";
    public bool Parcial { get; set; }
    public bool Anulado { get; set; }
    public string? MotivoAnulacao { get; set; }
    public bool Contabilizado { get; set; }
    public int? LancamentoId { get; set; }
}

public class PrestacaoContas
{
    public int Id { get; set; }
    public int SeguradoraId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
}

public class Sinistro
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int ApoliceId { get; set; }
    public DateTime DataOcorrencia { get; set; }
    public string Local { get; set; } = "";
    public string Descricao { get; set; } = "";
    public decimal ValorEstimado { get; set; }
    public string Estado { get; set; } = "Em análise";
}

public class ContaPlano
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Tipo { get; set; } = ""; // Activo | Passivo | Capital Próprio | Proveito | Custo
    public bool AceitaLancamentos { get; set; } = true;
}

public class LinhaLancamento
{
    public string Conta { get; set; } = "";
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
}

public class Lancamento
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = "";
    public string Origem { get; set; } = "Manual";
    public string Moeda { get; set; } = "MZN";
    public decimal Cambio { get; set; } = 1;
    public List<LinhaLancamento> Linhas { get; set; } = new();
}

public class CadastroItem
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Extra { get; set; } = "";
}

public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public DateTime Prazo { get; set; }
    public bool Concluida { get; set; }
}

/// <summary>
/// Dados MOCK em memória. TODO: substituir por API/base de dados real mantendo
/// os mesmos contratos usados por CorretoraService e pelos Controllers.
/// </summary>
public static class CorretoraMock
{
    public static readonly List<Cliente> Clientes = new();
    public static readonly List<Seguradora> Seguradoras = new();
    public static readonly List<Apolice> Apolices = new();
    public static readonly List<Debito> Debitos = new();
    public static readonly List<Pagamento> Pagamentos = new();
    public static readonly List<PrestacaoContas> Prestacoes = new();
    public static readonly List<Sinistro> Sinistros = new();
    public static readonly List<ContaPlano> Contas = new();
    public static readonly List<Lancamento> Lancamentos = new();
    public static readonly List<CadastroItem> Cadastros = new();
    public static readonly List<Tarefa> Tarefas = new();

    public static int Prox(IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;
    public static string Sequencia(string prefixo, int id) => $"{prefixo}/{DateTime.Today.Year}/{id:D4}";

    public static Cliente? Cli(int id) => Clientes.FirstOrDefault(x => x.Id == id);
    public static Seguradora? Seg(int id) => Seguradoras.FirstOrDefault(x => x.Id == id);
    public static Apolice? Apo(int id) => Apolices.FirstOrDefault(x => x.Id == id);
    public static Debito? Deb(int id) => Debitos.FirstOrDefault(x => x.Id == id);
    public static string NomeCliente(int id) => Cli(id)?.Nome ?? "—";
    public static string NomeSeg(int id) => Seg(id)?.Sigla ?? "—";

    public static decimal Comissao(Pagamento p)
    {
        var d = Deb(p.DebitoId);
        var s = d is null ? null : Seg(d.SeguradoraId);
        return s is null ? 0 : Math.Round(p.Valor * s.ComissaoPct / 100m, 2);
    }

    public static Lancamento NovoLancamento(DateTime data, string descricao, string origem, params (string Conta, decimal D, decimal C)[] linhas)
    {
        var l = new Lancamento { Id = Prox(Lancamentos.Select(x => x.Id)), Data = data, Descricao = descricao, Origem = origem };
        l.Numero = Sequencia("LC", l.Id);
        foreach (var x in linhas.Where(x => x.D != 0 || x.C != 0))
            l.Linhas.Add(new LinhaLancamento { Conta = x.Conta, Debito = x.D, Credito = x.C });
        Lancamentos.Add(l);
        return l;
    }

    public static Pagamento RegistarPagamento(Debito d, decimal valor, string metodo, DateTime data)
    {
        var id = Prox(Pagamentos.Select(x => x.Id));
        d.Pago += valor;
        var p = new Pagamento { Id = id, Numero = Sequencia("RC", id), DebitoId = d.Id, Data = data, Valor = valor, Metodo = metodo, Parcial = d.Pago < d.Valor };
        Pagamentos.Add(p);
        return p;
    }

    public static Lancamento ContabilizarPagamento(Pagamento p)
    {
        var d = Deb(p.DebitoId)!;
        var com = Comissao(p);
        var conta = p.Metodo == "Numerário" ? "11" : "12";
        var l = NovoLancamento(p.Data, $"Recebimento {p.Numero} — {NomeCliente(d.ClienteId)}", "Caixa",
            (conta, p.Valor, 0), ("22", 0, p.Valor - com), ("71", 0, com));
        p.Contabilizado = true;
        p.LancamentoId = l.Id;
        return l;
    }

    static CorretoraMock() => Semear();

    private static void Cad(string tipo, string cod, string desc, string extra = "") =>
        Cadastros.Add(new CadastroItem { Id = Prox(Cadastros.Select(x => x.Id)), Tipo = tipo, Codigo = cod, Descricao = desc, Extra = extra });

    private static void Cli(int id, string tipo, string nome, string nuit, string tdoc, string ndoc, string tel, string email, string end, int regOff) =>
        Clientes.Add(new Cliente { Id = id, Codigo = $"CL{id:D4}", Tipo = tipo, Nome = nome, Nuit = nuit, TipoDocumento = tdoc, NumDocumento = ndoc, Telefone = tel, Email = email, Endereco = end, DataRegisto = DateTime.Today.AddDays(regOff) });

    private static void Conta(string cod, string nome, string tipo, bool lanca = true) =>
        Contas.Add(new ContaPlano { Id = Prox(Contas.Select(x => x.Id)), Codigo = cod, Nome = nome, Tipo = tipo, AceitaLancamentos = lanca });

    private static void Ap(int cli, int seg, string cod, string ramo, string tipo, string obj, decimal premio, decimal capital,
        int inicioOff, string estado, int vencOff, decimal pago, int pagoOff = 0, bool emitido = true, bool anulada = false)
    {
        var id = Prox(Apolices.Select(a => a.Id));
        var ini = DateTime.Today.AddDays(inicioOff);
        var ap = new Apolice
        {
            Id = id, Numero = $"AP/{cod}/{DateTime.Today.Year}/{id:D4}", ClienteId = cli, SeguradoraId = seg,
            Ramo = ramo, TipoSeguro = tipo, Objecto = obj, Capital = capital, Premio = premio,
            Inicio = ini, Fim = ini.AddYears(1).AddDays(-1), Estado = estado
        };
        if (estado == "Anulada") { ap.DataAnulacao = DateTime.Today.AddDays(-90); ap.MotivoAnulacao = "Pedido do cliente"; }
        Apolices.Add(ap);

        var did = Prox(Debitos.Select(x => x.Id));
        var deb = new Debito
        {
            Id = did, Numero = Sequencia("DB", did), ApoliceId = id, ClienteId = cli, SeguradoraId = seg, Tipo = "Prémio",
            Descricao = $"Prémio {ramo} — {obj}", Valor = premio, Emissao = emitido ? ini : DateTime.Today,
            Vencimento = DateTime.Today.AddDays(vencOff), Emitido = emitido, Anulado = anulada
        };
        Debitos.Add(deb);

        if (pago > 0)
        {
            var p = RegistarPagamento(deb, pago, "Transferência", DateTime.Today.AddDays(pagoOff));
            if (pagoOff < 0) ContabilizarPagamento(p); // o pagamento de hoje fica por lançar (para testar)
        }
    }

    private static void Semear()
    {
        var h = DateTime.Today;

        Seguradoras.AddRange(new[]
        {
            new Seguradora { Id = 1, Nome = "Empresa Moçambicana de Seguros", Sigla = "EMOSE", Nuit = "400012345", Telefone = "+258 21 300 100", ComissaoPct = 12 },
            new Seguradora { Id = 2, Nome = "Hollard Moçambique", Sigla = "HOLLARD", Nuit = "400023456", Telefone = "+258 21 320 200", ComissaoPct = 15 },
            new Seguradora { Id = 3, Nome = "Fidelidade Moçambique", Sigla = "FIDELIDADE", Nuit = "400034567", Telefone = "+258 21 340 300", ComissaoPct = 14 },
            new Seguradora { Id = 4, Nome = "Global Alliance Seguros", Sigla = "GLOBAL", Nuit = "400045678", Telefone = "+258 21 350 400", ComissaoPct = 13 },
            new Seguradora { Id = 5, Nome = "Seguradora Internacional de Moçambique", Sigla = "SIM", Nuit = "400056789", Telefone = "+258 21 360 500", ComissaoPct = 12 },
        });

        Cli(1, "Particular", "Ana Maria Sitoe", "100200301", "BI", "110100123456B", "+258 84 123 4567", "ana.sitoe@email.co.mz", "Av. 24 de Julho, 1200, Maputo", -400);
        Cli(2, "Empresa", "Hotel Maputo Bay, SA", "400900101", "NUIT", "400900101", "+258 21 480 000", "geral@maputobay.co.mz", "Av. Marginal, 55, Maputo", -380);
        Cli(3, "Empresa", "Construções Zambeze, Lda", "400900202", "NUIT", "400900202", "+258 82 222 3333", "obras@zambeze.co.mz", "Matola, Parcela 18", -300);
        Cli(4, "Particular", "Carlos Nhaca", "100200404", "BI", "110100654321C", "+258 86 765 4321", "carlos.nhaca@email.co.mz", "Bairro Triunfo, Maputo", -9);
        Cli(5, "Particular", "Fátima Cossa", "100200505", "Passaporte", "AB123456", "+258 87 888 9999", "fatima.cossa@email.co.mz", "Polana Cimento, Maputo", -3);
        Cli(6, "Empresa", "Transportes Limpopo, Lda", "400900606", "NUIT", "400900606", "+258 82 555 6666", "frota@limpopo.co.mz", "Zona Industrial da Beira", -250);

        Cad("ramos", "AUT", "Automóvel"); Cad("ramos", "INC", "Incêndio e Riscos Diversos"); Cad("ramos", "SAU", "Saúde");
        Cad("ramos", "ACT", "Acidentes de Trabalho"); Cad("ramos", "TRA", "Transportes"); Cad("ramos", "VID", "Vida");
        Cad("tipos-seguro", "RCA", "Responsabilidade Civil (Auto)", "Automóvel"); Cad("tipos-seguro", "TRA1", "Todos os Riscos (Auto)", "Automóvel");
        Cad("tipos-seguro", "MRC", "Multirriscos Comercial", "Incêndio e Riscos Diversos"); Cad("tipos-seguro", "PSE", "Plano Saúde Empresarial", "Saúde");
        Cad("tipos-seguro", "ATR", "Acidentes de Trabalho", "Acidentes de Trabalho"); Cad("tipos-seguro", "MTR", "Mercadorias Transportadas", "Transportes");
        Cad("tipos-seguro", "VRI", "Vida Risco", "Vida");
        Cad("coberturas", "COB01", "Danos a Terceiros", "Automóvel"); Cad("coberturas", "COB02", "Roubo e Furto", "Automóvel");
        Cad("coberturas", "COB03", "Colisão e Capotamento", "Automóvel"); Cad("coberturas", "COB04", "Incêndio e Explosão", "Incêndio e Riscos Diversos");
        Cad("coberturas", "COB05", "Assistência Médica e Medicamentosa", "Saúde");
        Cad("balcoes", "B01", "Balcão Maputo Cidade", "EMOSE"); Cad("balcoes", "B02", "Balcão Matola", "HOLLARD"); Cad("balcoes", "B03", "Balcão Beira", "FIDELIDADE");
        Cad("bancos", "BCI", "Banco Comercial e de Investimentos", "BCIMMZMX"); Cad("bancos", "BIM", "Millennium bim", "BIMOMZMX");
        Cad("bancos", "SBM", "Standard Bank", "SBICMZMX"); Cad("bancos", "ABS", "Absa Bank Moçambique", "BARCMZMX");
        Cad("tipos-cliente", "PAR", "Particular"); Cad("tipos-cliente", "EMP", "Empresa"); Cad("tipos-cliente", "INS", "Instituição Pública");
        Cad("tipos-contacto", "TLM", "Telemóvel"); Cad("tipos-contacto", "TEL", "Telefone Fixo"); Cad("tipos-contacto", "EML", "Email"); Cad("tipos-contacto", "WPP", "WhatsApp");
        Cad("tipos-documento", "BI", "BI"); Cad("tipos-documento", "PAS", "Passaporte"); Cad("tipos-documento", "DIRE", "DIRE"); Cad("tipos-documento", "NUIT", "NUIT");
        Cad("tipos-documento", "CC", "Carta de Condução");
        Cad("cambios", "USD", "Dólar Americano", "63.90"); Cad("cambios", "EUR", "Euro", "69.50"); Cad("cambios", "ZAR", "Rand Sul-Africano", "3.55");
        Cad("contas-bancarias", "CB01", "Millennium bim — Conta Corrente", "0001 0000 12345678 01");
        Cad("contas-bancarias", "CB02", "BCI — Conta Corrente", "0008 0000 87654321 02");
        Cad("selos", "SEL01", "Selo de Apólice", "50.00"); Cad("selos", "SEL02", "Selo de Recibo", "10.00");
        Cad("comissoes", "COM01", "Comissão Automóvel", "12"); Cad("comissoes", "COM02", "Comissão Incêndio e Riscos Diversos", "15");
        Cad("comissoes", "COM03", "Comissão Saúde", "14"); Cad("comissoes", "COM04", "Comissão Vida", "10");

        Conta("11", "Caixa", "Activo"); Conta("12", "Depósitos à Ordem", "Activo");
        Conta("21", "Clientes c/c", "Activo"); Conta("22", "Seguradoras c/c", "Passivo");
        Conta("24", "Estado — Impostos e Selos", "Passivo"); Conta("51", "Capital Social", "Capital Próprio");
        Conta("61", "Gastos com Pessoal", "Custo"); Conta("62", "Fornecimentos e Serviços Externos", "Custo");
        Conta("71", "Comissões de Corretagem", "Proveito"); Conta("72", "Outros Proveitos", "Proveito");
        Conta("81", "Resultado Líquido do Exercício", "Capital Próprio");
        Conta("1", "Classe 1 — Meios Financeiros", "Activo", false);

        NovoLancamento(h.AddDays(-200), "Realização de capital social", "Manual", ("12", 2000000, 0), ("51", 0, 2000000));
        NovoLancamento(h.AddDays(-150), "Pagamento de rendas e serviços", "Manual", ("62", 25000, 0), ("11", 0, 25000));
        NovoLancamento(h.AddDays(-120), "Salários do mês", "Manual", ("61", 80000, 0), ("12", 0, 80000));

        Ap(1, 1, "AUT", "Automóvel", "Todos os Riscos (Auto)", "Toyota Hilux — MMD-123-MC", 48500, 1500000, -340, "Activa", -335, 48500, -335);
        Ap(2, 2, "INC", "Incêndio e Riscos Diversos", "Multirriscos Comercial", "Hotel Maputo Bay — Edifício principal", 285000, 25000000, -30, "Activa", 12, 0);
        Ap(3, 3, "SAU", "Saúde", "Plano Saúde Empresarial", "Construções Zambeze — 24 colaboradores", 96000, 0, -60, "Activa", -10, 30000, 0);
        Ap(4, 1, "AUT", "Automóvel", "Responsabilidade Civil (Auto)", "Mazda CX-5 — MCC-456-MP", 32500, 900000, -5, "Activa", 20, 0);
        Ap(6, 4, "TRA", "Transportes", "Mercadorias Transportadas", "Frota de 6 camiões — Maputo/Beira", 175000, 12000000, -350, "Activa", -345, 175000, -345);
        Ap(3, 5, "ACT", "Acidentes de Trabalho", "Acidentes de Trabalho", "Construções Zambeze — 60 trabalhadores", 64000, 0, -2, "Activa", 28, 0);
        Ap(5, 2, "AUT", "Automóvel", "Todos os Riscos (Auto)", "Nissan Navara — MBX-789-MC", 41000, 1100000, -200, "Activa", -190, 41000, -190);
        Ap(1, 3, "INC", "Incêndio e Riscos Diversos", "Multirriscos Comercial", "Residência — Matola", 22000, 3500000, 0, "Pendente", 30, 0, 0, false);
        Ap(3, 1, "AUT", "Automóvel", "Responsabilidade Civil (Auto)", "Toyota Corolla — MAB-321-MC", 38000, 800000, -100, "Anulada", -95, 0, 0, true, true);
        Ap(4, 5, "VID", "Vida", "Vida Risco", "Seguro de vida — Carlos Nhaca", 18000, 500000, 0, "Pendente", 25, 0, 0, false);

        Sinistros.Add(new Sinistro { Id = 1, Numero = Sequencia("SN", 1), ApoliceId = 1, DataOcorrencia = h.AddDays(-12), Local = "EN1, Marracuene", Descricao = "Colisão traseira com outra viatura.", ValorEstimado = 85000, Estado = "Em análise" });
        Sinistros.Add(new Sinistro { Id = 2, Numero = Sequencia("SN", 2), ApoliceId = 7, DataOcorrencia = h.AddDays(-45), Local = "Av. Julius Nyerere, Maputo", Descricao = "Furto de acessórios da viatura.", ValorEstimado = 22000, Estado = "Aprovado" });

        Tarefas.Add(new Tarefa { Id = 1, Titulo = "Contactar clientes com seguros a renovar", Prazo = h.AddDays(3) });
        Tarefas.Add(new Tarefa { Id = 2, Titulo = "Enviar prestação de contas à EMOSE", Prazo = h.AddDays(7) });
        Tarefas.Add(new Tarefa { Id = 3, Titulo = "Conferir caixa do dia", Prazo = h, Concluida = true });
    }
}
