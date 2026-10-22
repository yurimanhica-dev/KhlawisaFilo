namespace Klahisa.Web.Shared.Helpers;

public static class Fmt
{
    public static string Mt(decimal v) => v.ToString("N2") + " MT";
    public static string Dt(DateTime d) => d.ToString("dd/MM/yyyy");
    public static string Dt(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "—";
    public static string Num(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
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
