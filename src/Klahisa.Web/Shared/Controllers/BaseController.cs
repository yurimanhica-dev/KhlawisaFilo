namespace Klahisa.Web.Shared.Controllers;

public abstract class BaseController : Controller
{
    protected IActionResult Tabela(TabelaVM vm) => View("Tabela", vm);
    protected IActionResult Form(FormVM vm) => View("Formulario", vm);

    protected IActionResult Sucesso(string msg, string url) { TempData["Sucesso"] = msg; return Redirect(url); }
    protected IActionResult Falha(string msg, string url) { TempData["Erro"] = msg; return Redirect(url); }

    protected IActionResult FormComErro(FormVM vm, IFormCollection f, string erro)
    {
        Preencher(vm, f);
        ModelState.AddModelError(string.Empty, erro);
        return Form(vm);
    }

    protected static void Preencher(FormVM vm, IFormCollection f)
    {
        foreach (var c in vm.Campos)
            if (!c.SoLeitura && f.ContainsKey(c.Nome)) c.Valor = f[c.Nome].ToString();
    }

    protected static decimal Dec(string? s) =>
        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0;

    protected static int Int(string? s) => int.TryParse(s, out var v) ? v : 0;

    protected static DateTime Dt(string? s, DateTime? padrao = null) =>
        DateTime.TryParseExact(s?.Trim(), new[] { "yyyy-MM-dd", "dd/MM/yyyy" }, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var v) ? v : (padrao ?? DateTime.Today);
}
