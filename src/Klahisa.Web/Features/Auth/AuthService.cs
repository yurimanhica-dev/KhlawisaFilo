namespace Klahisa.Web.Features.Auth;

public class AuthService
{
    private readonly IAuthApiClient _client;
    public AuthService(IAuthApiClient client) => _client = client;

    public Task<Result<UsuarioAutenticado>> EntrarAsync(string email, string senha) => _client.LoginAsync(email, senha);
    public Task<Result<bool>> AlterarSenhaAsync(string email, string atual, string nova) => _client.AlterarSenhaAsync(email, atual, nova);
}
