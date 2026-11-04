using System.Net;
using System.Text;

namespace Klahisa.Web.Shared.Helpers;

public static class Combos
{
    public static string Param(string nome, string tipo, int actual, bool obrigatorio, bool mostrarValor = false, string id = "")
    {
        var sb = new StringBuilder();
        sb.Append($"<select name=\"{nome}\" id=\"{(id == "" ? nome : id)}\" class=\"form-control\"{(obrigatorio ? " required" : "")}>");
        sb.Append(obrigatorio ? "<option value=\"\">— Seleccione —</option>" : "<option value=\"0\">—</option>");

        foreach (var p in M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Descricao))
        {
            var valor = p.Coeficiente > 0 ? p.Coeficiente : 0m;
            var texto = WebUtility.HtmlEncode(p.Descricao) + (mostrarValor && valor > 0 ? " (" + Fmt.Num(valor) + ")" : "");
            sb.Append("<option value=\"").Append(p.Id).Append("\" data-valor=\"").Append(valor.ToString("0.00", CultureInfo.InvariantCulture))
              .Append("\" data-letra=\"").Append(WebUtility.HtmlEncode(p.Codigo)).Append('"').Append(p.Id == actual ? " selected" : "")
              .Append('>').Append(texto).Append("</option>");
        }

        sb.Append("</select>");
        return sb.ToString();
    }
}
