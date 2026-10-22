using Microsoft.AspNetCore.Mvc;

namespace Klahisa.Web.Shared.Services;

public class NotifItem
{
    public string Url { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Documento { get; set; } = "";
    public decimal Valor { get; set; }
    public int Dias { get; set; }
}

public class NotificacoesVM
{
    public List<NotifItem> Vencer { get; set; } = new();
    public List<NotifItem> Emitir { get; set; } = new();
    public List<NotifItem> Renovar { get; set; } = new();
}

public class NotificacoesViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var h = DateTime.Today;
        return View(new NotificacoesVM
        {
            Vencer = M.Debitos.Where(d => d.AReceber && d.Vencimento >= h && d.Vencimento <= h.AddDays(30))
                .OrderBy(d => d.Vencimento)
                .Select(d => new NotifItem { Url = $"/Caixa/Pagar/{d.Id}", Cliente = M.NomeCliente(d.ClienteId), Documento = d.Numero, Valor = d.Saldo, Dias = (d.Vencimento - h).Days }).ToList(),
            Emitir = M.Debitos.Where(d => !d.Emitido && !d.Anulado)
                .Select(d => new NotifItem { Url = $"/Seguros/PrepararAlocacao/{d.Id}", Cliente = M.NomeCliente(d.ClienteId), Documento = d.Numero, Valor = d.Valor, Dias = (d.Vencimento - h).Days }).ToList(),
            Renovar = M.Apolices.Where(a => a.Estado == "Activa" && a.Fim >= h && a.Fim <= h.AddDays(60))
                .OrderBy(a => a.Fim)
                .Select(a => new NotifItem { Url = $"/Seguros/Renovar/{a.Id}", Cliente = M.NomeCliente(a.ClienteId), Documento = a.Numero, Valor = a.Premio, Dias = (a.Fim - h).Days }).ToList()
        });
    }
}
