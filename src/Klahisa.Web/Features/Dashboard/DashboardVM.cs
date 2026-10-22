namespace Klahisa.Web.Features.Dashboard;

public class DashboardVM
{
    public string MesActual { get; set; } = "";
    public int ClientesMes { get; set; }
    public int ClientesTotal { get; set; }
    public int Pedidos { get; set; }
    public int PedidosTotal { get; set; }
    public int Apolices { get; set; }
    public int ApolicesTotal { get; set; }
    public int SinistrosMes { get; set; }
    public int SinistrosTotal { get; set; }
    public string[] Meses { get; set; } = Array.Empty<string>();
    public decimal[] Emitidos { get; set; } = Array.Empty<decimal>();
    public decimal[] Pagos { get; set; } = Array.Empty<decimal>();
    public decimal[] Comissao { get; set; } = Array.Empty<decimal>();
    public List<Debito> Vencidos { get; set; } = new();
    public List<Apolice> Ultimas { get; set; } = new();
}
