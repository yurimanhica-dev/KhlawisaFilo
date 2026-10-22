namespace Klahisa.Web.Features.Dashboard;

public class DashboardController : BaseController
{
    public IActionResult Index()
    {
        var h = DateTime.Today;
        var meses = new[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

        var vm = new DashboardVM
        {
            MesActual = meses[h.Month - 1].ToUpper(),
            ClientesMes = M.Clientes.Count(c => c.DataRegisto.Year == h.Year && c.DataRegisto.Month == h.Month),
            ClientesTotal = M.Clientes.Count,
            Pedidos = M.Apolices.Count(a => a.Estado == "Pendente" && a.Inicio.Month == h.Month && a.Inicio.Year == h.Year),
            PedidosTotal = M.Apolices.Count(a => a.Estado == "Pendente"),
            Apolices = M.Apolices.Count(a => a.Estado == "Activa" && a.Inicio.Month == h.Month && a.Inicio.Year == h.Year),
            ApolicesTotal = M.Apolices.Count(a => a.Estado == "Activa"),
            SinistrosMes = M.Sinistros.Count(s => s.DataOcorrencia.Month == h.Month && s.DataOcorrencia.Year == h.Year),
            SinistrosTotal = M.Sinistros.Count,
            Meses = meses,
            Emitidos = Enumerable.Range(1, 12).Select(m => M.Apolices.Where(a => a.Estado != "Anulada" && a.Inicio.Year == h.Year && a.Inicio.Month == m).Sum(a => a.Premio)).ToArray(),
            Pagos = Enumerable.Range(1, 12).Select(m => M.Pagamentos.Where(p => !p.Anulado && p.Data.Year == h.Year && p.Data.Month == m).Sum(p => p.Valor)).ToArray(),
            Comissao = Enumerable.Range(1, 12).Select(m => M.Pagamentos.Where(p => !p.Anulado && p.Data.Year == h.Year && p.Data.Month == m).Sum(p => M.Comissao(p))).ToArray(),
            Vencidos = M.Debitos.Where(d => d.Estado == "Vencido").OrderBy(d => d.Vencimento).Take(5).ToList(),
            Ultimas = M.Apolices.OrderByDescending(a => a.Id).Take(5).ToList()
        };
        return View(vm);
    }
}
