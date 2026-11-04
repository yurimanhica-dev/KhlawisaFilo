namespace Klahisa.Web.Shared.Models;

public class AcaoVM
{
    public string Texto { get; set; } = "";
    public string Url { get; set; } = "";
    public string Icone { get; set; } = "";
    public string Css { get; set; } = "btn-primary";
    public bool Post { get; set; }
    public string? Confirmar { get; set; }

    public static AcaoVM Link(string texto, string url, string icone = "", string css = "btn-primary")
        => new() { Texto = texto, Url = url, Icone = icone, Css = css };

    public static AcaoVM Envio(string texto, string url, string? confirmar = null, string icone = "", string css = "btn-primary")
        => new() { Texto = texto, Url = url, Post = true, Confirmar = confirmar, Icone = icone, Css = css };
}

public class LinhaVM
{
    public List<string> Celulas { get; set; } = new();
    public List<AcaoVM> Acoes { get; set; } = new();

    public static LinhaVM De(params string[] celulas) => new() { Celulas = celulas.ToList() };

    public LinhaVM Com(params AcaoVM[] acoes)
    {
        Acoes.AddRange(acoes);
        return this;
    }
}

public class FiltroVM
{
    public string Nome { get; set; } = "";
    public string Rotulo { get; set; } = "";
    public string Url { get; set; } = "";
    public string Valor { get; set; } = "";
    public List<(string Valor, string Texto)> Opcoes { get; set; } = new();
}

public class TabelaVM
{
    public string Titulo { get; set; } = "";
    public string Subtitulo { get; set; } = "";
    public List<string> Colunas { get; set; } = new();
    public List<LinhaVM> Linhas { get; set; } = new();
    public List<AcaoVM> AcoesTopo { get; set; } = new();
    public List<(string Rotulo, string Valor)> Resumo { get; set; } = new();
    public FiltroVM? Filtro { get; set; }
    public string Vazio { get; set; } = "Sem registos para mostrar.";
    public bool Pesquisavel { get; set; } = true;
}

public class CampoVM
{
    public string Nome { get; set; } = "";
    public string Rotulo { get; set; } = "";
    public string Tipo { get; set; } = "text";
    public string Valor { get; set; } = "";
    public bool Obrigatorio { get; set; } = true;
    public bool SoLeitura { get; set; }
    public bool CartaoLateral { get; set; }
    public bool Pesquisavel { get; set; }
    public bool ValidacaoDecimal { get; set; }
    public int Largura { get; set; } = 6;
    public string Dica { get; set; } = "";
    public bool SoDigitos { get; set; }
    public int MinDigitos { get; set; }
    public int MaxDigitos { get; set; }
    public string Secao { get; set; } = "";
    public string Aba { get; set; } = "";
    public string? MostrarParaTipo { get; set; }
    public string DependeDe { get; set; } = "";
    public Dictionary<string, string> OpcaoPai { get; set; } = new();
    public Dictionary<string, string> DadosOpcao { get; set; } = new();
    public List<(string Valor, string Texto)> Opcoes { get; set; } = new();

    public static CampoVM Txt(string n, string r, bool obrig = true, int larg = 6, string valor = "", string tipo = "text")
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Largura = larg, Valor = valor, Tipo = tipo };

    public static CampoVM Area(string n, string r, bool obrig = true)
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Largura = 12, Tipo = "textarea" };

    public static CampoVM Dig(string n, string r, int min, int max, bool obrig = true, int larg = 6)
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Largura = larg, Tipo = "text", SoDigitos = true, MinDigitos = min, MaxDigitos = max };

    public static CampoVM Num(string n, string r, string valor = "", bool obrig = true, int larg = 6)
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Largura = larg, Valor = valor, Tipo = "text", ValidacaoDecimal = true };   // era "number"
    public static CampoVM Dat(string n, string r, DateTime? v = null, bool obrig = true, int larg = 6)
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Tipo = "date", Largura = larg, Valor = v.HasValue ? v.Value.ToString("yyyy-MM-dd") : "" };

    public static CampoVM Sel(string n, string r, List<(string, string)> ops, string valor = "", bool obrig = true, int larg = 6)
        => new() { Nome = n, Rotulo = r, Obrigatorio = obrig, Tipo = "select", Valor = valor, Opcoes = ops, Largura = larg };

    public static CampoVM Ler(string n, string r, string valor, int larg = 6)
        => new() { Nome = n, Rotulo = r, Valor = valor, SoLeitura = true, Obrigatorio = false, Largura = larg };
}

public class FormVM
{
    public string Titulo { get; set; } = "";
    public string Subtitulo { get; set; } = "";
    public string ActionUrl { get; set; } = "";
    public string VoltarUrl { get; set; } = "";
    public string Botao { get; set; } = "Guardar";
    public string Info { get; set; } = "";
    public bool UsaAbas { get; set; }
    public List<CampoVM> Campos { get; set; } = new();
}
