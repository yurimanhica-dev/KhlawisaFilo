namespace Klahisa.Web.Features.Estatisticas;

public class EstatisticasController : BaseController
{
    private readonly CorretoraService _svc;
    public EstatisticasController(CorretoraService svc) => _svc = svc;

    public IActionResult RelacaoSeguradoras()
    {
        var vm = new TabelaVM
        {
            Titulo = "Relação de Seguradoras", Subtitulo = "Produção por seguradora",
            Colunas = { "Seguradora", "NUIT", "Apólices", "Prémios emitidos", "Recebido", "Comissão", "Saldo a entregar" }
        };
        foreach (var s in M.Seguradoras)
        {
            var ap = M.Apolices.Where(a => a.SeguradoraId == s.Id && a.Estado != "Anulada").ToList();
            var x = _svc.SaldoSeguradora(s.Id);
            vm.Linhas.Add(LinhaVM.De(s.Nome, s.Nuit, ap.Count.ToString(), Fmt.Mt(ap.Sum(a => a.Premio)), Fmt.Mt(x.Recebido), Fmt.Mt(x.Comissao), Fmt.Mt(x.Saldo)));
        }
        return Tabela(vm);
    }

    public IActionResult EstatisticasDiarias()
    {
        var vm = new TabelaVM
        {
            Titulo = "Estatísticas Diárias", Subtitulo = "Últimos 7 dias", Pesquisavel = false,
            Colunas = { "Dia", "Novos clientes", "Pedidos / apólices", "Recibos", "Valor recebido", "Sinistros" }
        };
        for (var i = 0; i < 7; i++)
        {
            var d = DateTime.Today.AddDays(-i);
            var pags = M.Pagamentos.Where(p => !p.Anulado && p.Data.Date == d).ToList();
            vm.Linhas.Add(LinhaVM.De(Fmt.Dt(d), M.Clientes.Count(c => c.DataRegisto.Date == d).ToString(), M.Apolices.Count(a => a.Inicio.Date == d).ToString(),
                pags.Count.ToString(), Fmt.Mt(pags.Sum(p => p.Valor)), M.Sinistros.Count(s => s.DataOcorrencia.Date == d).ToString()));
        }
        return Tabela(vm);
    }

    public IActionResult MapaISSM()
    {
        var vm = new TabelaVM
        {
            Titulo = "Mapa ISSM", Subtitulo = "Mapa de produção por ramo — Instituto de Supervisão de Seguros de Moçambique", Pesquisavel = false,
            Colunas = { "Ramo", "Nº apólices", "Capital seguro", "Prémios", "Comissões", "Sinistros", "Valor sinistros" }
        };
        foreach (var r in M.Cadastros.Where(c => c.Tipo == "ramos"))
        {
            var ap = M.Apolices.Where(a => a.Ramo == r.Descricao && a.Estado != "Anulada").ToList();
            var ids = ap.Select(a => a.Id).ToList();
            var sin = M.Sinistros.Where(s => ids.Contains(s.ApoliceId)).ToList();
            var com = M.Pagamentos.Where(p => !p.Anulado && ids.Contains(M.Deb(p.DebitoId)?.ApoliceId ?? 0)).Sum(p => M.Comissao(p));
            vm.Linhas.Add(LinhaVM.De(r.Descricao, ap.Count.ToString(), Fmt.Mt(ap.Sum(a => a.Capital)), Fmt.Mt(ap.Sum(a => a.Premio)), Fmt.Mt(com), sin.Count.ToString(), Fmt.Mt(sin.Sum(s => s.ValorEstimado))));
        }
        return Tabela(vm);
    }
}
