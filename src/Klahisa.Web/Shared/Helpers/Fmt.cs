
namespace Klahisa.Web.Shared.Helpers;

public static class Fmt
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly CultureInfo Pt = new("pt-PT");

    public const string FormatoData = "dd/MM/yyyy";
    public const string FormatoDataExtenso = "dd/MMMM/yyyy";

    public static string Num(decimal v) => v.ToString("N2", Inv);
    public static string Num(decimal? v) => (v ?? 0m).ToString("N2", Inv);
    public static string Num(string? s) => Parse(s).ToString("N2", Inv);

    public static string Mt(decimal v) => Num(v) + " MT";
    public static string Mt(decimal? v) => Num(v) + " MT";

    public static decimal Parse(string? s) =>
        decimal.TryParse(s?.Trim(), NumberStyles.Any, Inv, out var v) ? v : 0m;

    public static string Dias(decimal v) => v.ToString("0.##", Inv);
    public static string Pct(decimal v) => v.ToString("0.##", Inv) + " %";

    public static string Dt(DateTime d) => d.ToString(FormatoData, Pt);
    public static string Dt(DateTime? d) => d.HasValue ? Dt(d.Value) : "—";
    public static string DtExtenso(DateTime d) => d.ToString(FormatoDataExtenso, Pt);
    public static string DtHora(DateTime d) => d.ToString(FormatoData + " HH:mm", Pt);
    public static string Input(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd", Inv) : "";

    private static readonly string[] Meses =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    public static string Mes(int m) => m is >= 1 and <= 12 ? Meses[m - 1] : "—";
    public static string[] NomesMeses => Meses;
    public static string MesAno(int m, int a) => $"{Mes(m)} / {a}";
}

public static class Opcoes
{
    public static List<(string, string)> Clientes() =>
        M.Clientes.Select(c => (c.Id.ToString(), $"{c.Codigo} — {c.Nome}")).ToList();

    public static List<(string, string)> Seguradoras() =>
        M.Seguradoras.Select(s => (s.Id.ToString(), $"{s.Sigla} — {s.Nome}")).ToList();

    public static List<(string, string)> Apolices(bool soActivas = false) =>
        M.Apolices.Where(a => !soActivas || a.Estado == "Activa")
            .Select(a => (a.Id.ToString(), $"{a.Numero} — {M.NomeCliente(a.ClienteId)}")).ToList();

    public static List<(string, string)> Cad(string tipo) =>
        M.Cadastros.Where(c => c.Tipo == tipo).Select(c => (c.Descricao, c.Descricao)).ToList();

    public static List<(string, string)> CadastrosPorId(string tipo) =>
        M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Descricao).Select(c => (c.Id.ToString(), c.Descricao)).ToList();

    public static List<(string, string)> Moedas()
    {
        var l = new List<(string, string)> { ("MZN", "MZN — Metical") };
        l.AddRange(M.Cadastros.Where(c => c.Tipo == "cambios").Select(c => (c.Codigo, $"{c.Codigo} — {c.Descricao}")));
        return l;
    }

    public static List<(string, string)> Contas(bool vazio = false, bool soLancaveis = true)
    {
        var l = new List<(string, string)>();
        if (vazio) l.Add(("", "—"));
        l.AddRange(M.Contas.Where(c => !soLancaveis || c.AceitaLancamentos).OrderBy(c => c.Codigo)
            .Select(c => (c.Codigo, $"{c.Codigo} — {c.Nome}")));
        return l;
    }

    public static List<(string, string)> De(params string[] valores) => valores.Select(v => (v, v)).ToList();
}

public static class Opc
{
    public static List<(string, string)> Clientes() => Opcoes.Clientes();
    public static List<(string, string)> Seguradoras() => Opcoes.Seguradoras();
    public static List<(string, string)> Apolices(bool soActivas = false) => Opcoes.Apolices(soActivas);
    public static List<(string, string)> Cad(string tipo) => Opcoes.Cad(tipo);
    public static List<(string, string)> CadastrosPorId(string tipo) => Opcoes.CadastrosPorId(tipo);
    public static List<(string, string)> Moedas() => Opcoes.Moedas();
    public static List<(string, string)> Contas(bool vazio = false, bool soLancaveis = true) => Opcoes.Contas(vazio, soLancaveis);
    public static List<(string, string)> De(params string[] valores) => Opcoes.De(valores);
}
