using System.ComponentModel.DataAnnotations;

namespace Klahisa.Web.Features.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o seu email.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a sua senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}
