namespace Klahisa.Web.Features.Auth;

public class UsuarioAutenticado
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
}

public interface IAuthApiClient
{
    Task<Result<UsuarioAutenticado>> LoginAsync(string email, string senha);
    Task<Result<bool>> AlterarSenhaAsync(string email, string atual, string nova);
}
