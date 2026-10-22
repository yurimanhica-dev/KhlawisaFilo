using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Klahisa.Web.Features.Auth;

namespace Klahisa.Web.Features.Conta;

public class ContaController : BaseController
{
    private readonly AuthService _auth;
    public ContaController(AuthService auth) => _auth = auth;

    public IActionResult MeuPerfil() => Form(new FormVM
    {
        Titulo = "Meu Perfil", Subtitulo = "Dados do utilizador", ActionUrl = "/Conta/MeuPerfil", VoltarUrl = "/Dashboard/Index",
        Campos =
        {
            CampoVM.Ler("Nome", "Nome", User.Identity?.Name ?? ""),
            CampoVM.Ler("Email", "Email", User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? ""),
            CampoVM.Ler("Perfil", "Perfil", User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? ""),
            CampoVM.Ler("Empresa", "Empresa", "Klahisa Filo — Corretora de Seguros")
        }
    });

    [HttpPost]
    public IActionResult MeuPerfil(IFormCollection f) => Sucesso("Perfil guardado.", "/Conta/MeuPerfil");

    public IActionResult MinhasTarefas()
    {
        var vm = new TabelaVM { Titulo = "Minhas Tarefas", Subtitulo = "Tarefas atribuídas", Colunas = { "Tarefa", "Prazo", "Estado" } };
        foreach (var t in M.Tarefas.OrderBy(t => t.Concluida).ThenBy(t => t.Prazo))
        {
            var l = LinhaVM.De(t.Titulo, Fmt.Dt(t.Prazo), t.Concluida ? "Concluída" : "Pendente");
            if (!t.Concluida) l.Com(AcaoVM.Envio("Concluir", $"/Conta/ConcluirTarefa/{t.Id}", null, "ti-check"));
            vm.Linhas.Add(l);
        }
        return Tabela(vm);
    }

    [HttpPost]
    public IActionResult ConcluirTarefa(int id)
    {
        var t = M.Tarefas.FirstOrDefault(x => x.Id == id);
        if (t is not null) t.Concluida = true;
        return Sucesso("Tarefa concluída.", "/Conta/MinhasTarefas");
    }

    private static FormVM FormSenha() => new()
    {
        Titulo = "Alterar Senha", Subtitulo = "Segurança da conta", ActionUrl = "/Conta/AlterarSenha", VoltarUrl = "/Dashboard/Index", Botao = "Alterar senha",
        Campos =
        {
            CampoVM.Txt("Atual", "Senha actual", larg: 12, tipo: "password"),
            CampoVM.Txt("Nova", "Nova senha (mín. 8 caracteres)", tipo: "password"),
            CampoVM.Txt("Confirmar", "Confirmar nova senha", tipo: "password")
        }
    };

    [HttpGet] public IActionResult AlterarSenha() => Form(FormSenha());

    [HttpPost]
    public async Task<IActionResult> AlterarSenha(IFormCollection f)
    {
        var vm = FormSenha();
        if (f["Nova"] != f["Confirmar"]) { ModelState.AddModelError(string.Empty, "A confirmação não coincide com a nova senha."); return Form(vm); }
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var r = await _auth.AlterarSenhaAsync(email, f["Atual"].ToString(), f["Nova"].ToString());
        if (!r.IsSuccess) { ModelState.AddModelError(string.Empty, r.Error!); return Form(vm); }
        return Sucesso("Senha alterada com sucesso.", "/Dashboard/Index");
    }

    [HttpPost]
    public async Task<IActionResult> Bloquear()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Aviso"] = "Conta bloqueada. Inicie sessão para continuar.";
        return RedirectToAction("Login", "Auth");
    }
}
