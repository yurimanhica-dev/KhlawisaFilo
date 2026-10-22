using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Klahisa.Web.Features.Auth;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToAction("Index", "Dashboard");
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.EntrarAsync(model.Email, model.Senha);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        var u = result.Value!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, u.Nome),
            new(ClaimTypes.Email, u.Email),
            new(ClaimTypes.Role, u.Papel)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
