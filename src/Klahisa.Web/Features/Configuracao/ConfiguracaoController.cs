namespace Klahisa.Web.Features.Configuracao;

public class ConfiguracaoController : BaseController
{
    private readonly CorretoraService _svc;
    public ConfiguracaoController(CorretoraService svc) => _svc = svc;

    // tipo -> (título, rótulo do campo extra)
    private static readonly Dictionary<string, (string Titulo, string Extra)> Tipos = new()
    {
        ["bancos"] = ("Bancos", "SWIFT / BIC"),
        ["tipos-cliente"] = ("Tipos de Cliente", "Observação"),
        ["tipos-contacto"] = ("Tipos de Contacto", "Observação"),
        ["tipos-documento"] = ("Tipos de Documento de Identificação", "Observação"),
        ["cambios"] = ("Câmbios", "Taxa (MT por unidade)"),
        ["contas-bancarias"] = ("Contas Bancárias", "NIB"),
        ["ramos"] = ("Ramos", "Observação"),
        ["tipos-seguro"] = ("Tipos de Seguros", "Ramo"),
        ["coberturas"] = ("Coberturas", "Ramo"),
        ["balcoes"] = ("Balcões das Seguradoras", "Seguradora"),
        ["selos"] = ("Selos", "Valor (MT)"),
        ["comissoes"] = ("Comissões", "Percentagem (%)")
    };

    public IActionResult Cadastro(string tipo)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        var vm = new TabelaVM
        {
            Titulo = t.Titulo, Subtitulo = "Parametrização",
            Colunas = { "Código", "Descrição", t.Extra },
            AcoesTopo = { AcaoVM.Link("Novo registo", $"/Configuracao/CadastroNovo?tipo={tipo}", "ti-plus") }
        };
        foreach (var i in M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Codigo))
            vm.Linhas.Add(LinhaVM.De(i.Codigo, i.Descricao, i.Extra)
                .Com(AcaoVM.Envio("Eliminar", $"/Configuracao/CadastroEliminar/{i.Id}?tipo={tipo}", "Eliminar este registo?", "ti-trash", "btn-danger")));
        return Tabela(vm);
    }

    private static FormVM FormCad(string tipo, (string Titulo, string Extra) t) => new()
    {
        Titulo = $"{t.Titulo} — Novo registo", Subtitulo = "Parametrização", ActionUrl = $"/Configuracao/CadastroNovo?tipo={tipo}", VoltarUrl = $"/Configuracao/Cadastro?tipo={tipo}",
        Campos = { CampoVM.Txt("Codigo", "Código"), CampoVM.Txt("Descricao", "Descrição"), CampoVM.Txt("Extra", t.Extra, obrig: false, larg: 12) }
    };

    [HttpGet]
    public IActionResult CadastroNovo(string tipo) =>
        Tipos.TryGetValue(tipo ?? "", out var t) ? Form(FormCad(tipo!, t)) : Falha("Cadastro desconhecido.", "/Dashboard/Index");

    [HttpPost]
    public IActionResult CadastroNovo(string tipo, IFormCollection f)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        var r = _svc.GuardarCadastro(new CadastroItem { Tipo = tipo!, Codigo = f["Codigo"].ToString().Trim(), Descricao = f["Descricao"].ToString().Trim(), Extra = f["Extra"].ToString().Trim() });
        if (!r.IsSuccess) return FormComErro(FormCad(tipo!, t), f, r.Error!);
        return Sucesso("Registo criado.", $"/Configuracao/Cadastro?tipo={tipo}");
    }

    [HttpPost]
    public IActionResult CadastroEliminar(int id, string tipo)
    {
        var r = _svc.EliminarCadastro(id);
        return r.IsSuccess ? Sucesso("Registo eliminado.", $"/Configuracao/Cadastro?tipo={tipo}") : Falha(r.Error!, $"/Configuracao/Cadastro?tipo={tipo}");
    }

    // ======================= Seguradoras =======================
    public IActionResult Seguradoras()
    {
        var vm = new TabelaVM
        {
            Titulo = "Seguradoras", Subtitulo = "Seguradoras parceiras",
            Colunas = { "Sigla", "Nome", "NUIT", "Telefone", "Comissão (%)" },
            AcoesTopo = { AcaoVM.Link("Nova seguradora", "/Configuracao/SeguradoraNova", "ti-plus") }
        };
        foreach (var s in M.Seguradoras) vm.Linhas.Add(LinhaVM.De(s.Sigla, s.Nome, s.Nuit, s.Telefone, s.ComissaoPct.ToString("0.##")));
        return Tabela(vm);
    }

    private static FormVM FormSeg() => new()
    {
        Titulo = "Nova Seguradora", Subtitulo = "Parametrização", ActionUrl = "/Configuracao/SeguradoraNova", VoltarUrl = "/Configuracao/Seguradoras",
        Campos = { CampoVM.Txt("Nome", "Nome"), CampoVM.Txt("Sigla", "Sigla"), CampoVM.Txt("Nuit", "NUIT", obrig: false), CampoVM.Txt("Telefone", "Telefone", obrig: false), CampoVM.Num("Comissao", "Comissão (%)") }
    };

    [HttpGet] public IActionResult SeguradoraNova() => Form(FormSeg());

    [HttpPost]
    public IActionResult SeguradoraNova(IFormCollection f)
    {
        var r = _svc.GuardarSeguradora(new Seguradora { Nome = f["Nome"].ToString(), Sigla = f["Sigla"].ToString().ToUpper(), Nuit = f["Nuit"].ToString(), Telefone = f["Telefone"].ToString(), ComissaoPct = Dec(f["Comissao"]) });
        if (!r.IsSuccess) return FormComErro(FormSeg(), f, r.Error!);
        return Sucesso("Seguradora criada.", "/Configuracao/Seguradoras");
    }

    // ======================= Seguradoras e Clientes =======================
    public IActionResult SeguradorasClientes()
    {
        var vm = new TabelaVM
        {
            Titulo = "Seguradoras e Clientes", Subtitulo = "Carteira de clientes e comissão por seguradora",
            Colunas = { "Seguradora", "Comissão (%)", "Apólices activas", "Clientes", "Prémios activos" }
        };
        foreach (var s in M.Seguradoras)
        {
            var ap = M.Apolices.Where(a => a.SeguradoraId == s.Id && a.Estado == "Activa").ToList();
            vm.Linhas.Add(LinhaVM.De(s.Sigla, s.ComissaoPct.ToString("0.##"), ap.Count.ToString(), ap.Select(a => a.ClienteId).Distinct().Count().ToString(), Fmt.Mt(ap.Sum(a => a.Premio))));
        }
        return Tabela(vm);
    }
}
