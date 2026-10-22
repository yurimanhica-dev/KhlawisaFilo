namespace Klahisa.Web.Features.Clientes;

public class ClientesController : BaseController
{
    private readonly CorretoraService _svc;
    public ClientesController(CorretoraService svc) => _svc = svc;

    private static FormVM FormCliente() => new()
    {
        Titulo = "Cadastro de Cliente", Subtitulo = "Novo cliente", ActionUrl = "/Clientes/Novo", VoltarUrl = "/Clientes/Lista",
        Campos =
        {
            CampoVM.Sel("Tipo", "Tipo de cliente", Opcoes.Cad("tipos-cliente")),
            CampoVM.Txt("Nome", "Nome / Designação"),
            CampoVM.Txt("Nuit", "NUIT", obrig: false),
            CampoVM.Sel("TipoDocumento", "Documento de identificação", Opcoes.Cad("tipos-documento")),
            CampoVM.Txt("NumDocumento", "Nº do documento"),
            CampoVM.Txt("Telefone", "Telefone"),
            CampoVM.Txt("Email", "Email", obrig: false, tipo: "email"),
            CampoVM.Txt("Endereco", "Endereço", obrig: false, larg: 12)
        }
    };

    [HttpGet]
    public IActionResult Novo() => Form(FormCliente());

    [HttpPost]
    public IActionResult Novo(IFormCollection f)
    {
        var c = new Cliente
        {
            Tipo = f["Tipo"].ToString(), Nome = f["Nome"].ToString().Trim(), Nuit = f["Nuit"].ToString().Trim(),
            TipoDocumento = f["TipoDocumento"].ToString(), NumDocumento = f["NumDocumento"].ToString(),
            Telefone = f["Telefone"].ToString(), Email = f["Email"].ToString(), Endereco = f["Endereco"].ToString()
        };
        var r = _svc.RegistarCliente(c);
        if (!r.IsSuccess) return FormComErro(FormCliente(), f, r.Error!);
        return Sucesso($"Cliente {r.Value!.Codigo} registado com sucesso.", "/Clientes/Lista");
    }

    public IActionResult Lista()
    {
        var vm = new TabelaVM
        {
            Titulo = "Listagem de Clientes", Subtitulo = "Todos os clientes registados",
            Colunas = { "Código", "Nome", "Tipo", "NUIT", "Telefone", "Email", "Apólices", "Registo" },
            AcoesTopo = { AcaoVM.Link("Novo cliente", "/Clientes/Novo", "ti-plus") }
        };
        foreach (var c in M.Clientes.OrderBy(c => c.Id))
            vm.Linhas.Add(LinhaVM.De(c.Codigo, c.Nome, c.Tipo, c.Nuit, c.Telefone, c.Email,
                M.Apolices.Count(a => a.ClienteId == c.Id).ToString(), Fmt.Dt(c.DataRegisto)));
        return Tabela(vm);
    }
}
