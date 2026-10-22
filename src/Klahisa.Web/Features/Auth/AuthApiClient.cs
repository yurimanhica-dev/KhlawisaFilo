namespace Klahisa.Web.Features.Auth;

/// <summary>MOCK — utilizador de demonstração. TODO: trocar por HttpClient tipado quando existir API.</summary>
public class AuthApiClient : IAuthApiClient
{
    private static string _senhaAdmin = "Klahisa@2026";
    private const string EmailAdmin = "admin@klahisafilo.co.mz";

    public Task<Result<UsuarioAutenticado>> LoginAsync(string email, string senha)
    {
        var ok = email.Equals(EmailAdmin, StringComparison.OrdinalIgnoreCase) && senha == _senhaAdmin;
        return Task.FromResult(ok
            ? Result<UsuarioAutenticado>.Success(new UsuarioAutenticado { Nome = "Administrator", Email = EmailAdmin, Papel = "Administrador" })
            : Result<UsuarioAutenticado>.Failure("Email ou senha inválidos."));
    }

    public Task<Result<bool>> AlterarSenhaAsync(string email, string atual, string nova)
    {
        if (atual != _senhaAdmin) return Task.FromResult(Result<bool>.Failure("A senha actual está incorrecta."));
        if (nova.Length < 8) return Task.FromResult(Result<bool>.Failure("A nova senha deve ter pelo menos 8 caracteres."));
        _senhaAdmin = nova;
        return Task.FromResult(Result<bool>.Success(true));
    }
}
