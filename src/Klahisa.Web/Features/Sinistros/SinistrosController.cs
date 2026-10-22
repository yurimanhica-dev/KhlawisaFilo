namespace Klahisa.Web.Features.Sinistros;

public class SinistrosController : BaseController
{
    private readonly CorretoraService _svc;
    public SinistrosController(CorretoraService svc) => _svc = svc;

    private static FormVM FormOcorrencia() => new()
    {
        Titulo = "Ocorrência de Sinistro", Subtitulo = "Participar novo sinistro", ActionUrl = "/Sinistros/Ocorrencia", VoltarUrl = "/Sinistros/Lista",
        Campos =
        {
            CampoVM.Sel("ApoliceId", "Apólice", Opcoes.Apolices(true)),
            CampoVM.Dat("DataOcorrencia", "Data da ocorrência", DateTime.Today),
            CampoVM.Txt("Local", "Local da ocorrência"),
            CampoVM.Num("ValorEstimado", "Valor estimado (MT)", obrig: false),
            CampoVM.Area("Descricao", "Descrição da ocorrência")
        }
    };

    [HttpGet]
    public IActionResult Ocorrencia() => Form(FormOcorrencia());

    [HttpPost]
    public IActionResult Ocorrencia(IFormCollection f)
    {
        var r = _svc.RegistarSinistro(new Sinistro
        {
            ApoliceId = Int(f["ApoliceId"]), DataOcorrencia = Dt(f["DataOcorrencia"]), Local = f["Local"].ToString(),
            ValorEstimado = Dec(f["ValorEstimado"]), Descricao = f["Descricao"].ToString()
        });
        if (!r.IsSuccess) return FormComErro(FormOcorrencia(), f, r.Error!);
        return Sucesso($"Sinistro {r.Value!.Numero} registado.", "/Sinistros/Lista");
    }

    public IActionResult Lista()
    {
        var vm = new TabelaVM
        {
            Titulo = "Listagem de Sinistros", Subtitulo = "Ocorrências participadas",
            Colunas = { "Nº Sinistro", "Apólice", "Cliente", "Data", "Local", "Valor estimado", "Estado" },
            AcoesTopo = { AcaoVM.Link("Nova ocorrência", "/Sinistros/Ocorrencia", "ti-plus") }
        };
        foreach (var s in M.Sinistros.OrderByDescending(s => s.DataOcorrencia))
        {
            var a = M.Apo(s.ApoliceId);
            vm.Linhas.Add(LinhaVM.De(s.Numero, a?.Numero ?? "—", M.NomeCliente(a?.ClienteId ?? 0), Fmt.Dt(s.DataOcorrencia), s.Local, Fmt.Mt(s.ValorEstimado), s.Estado));
        }
        return Tabela(vm);
    }
}
