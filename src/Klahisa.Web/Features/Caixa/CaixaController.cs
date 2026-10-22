namespace Klahisa.Web.Features.Caixa;

public class CaixaController : BaseController
{
    private readonly CorretoraService _svc;
    public CaixaController(CorretoraService svc) => _svc = svc;

    // ======================= Lista de Débitos =======================
    public IActionResult ListaDebitos()
    {
        var vm = new TabelaVM
        {
            Titulo = "Lista de Débitos", Subtitulo = "Débitos emitidos — pagamento e edição",
            Colunas = { "Nº Débito", "Cliente", "Apólice", "Descrição", "Valor", "Pago", "Saldo", "Vencimento", "Estado" }
        };
        foreach (var d in M.Debitos.Where(d => d.Emitido && !d.Anulado).OrderBy(d => d.Vencimento))
        {
            var l = LinhaVM.De(d.Numero, M.NomeCliente(d.ClienteId), M.Apo(d.ApoliceId)?.Numero ?? "—", d.Descricao,
                Fmt.Mt(d.Valor), Fmt.Mt(d.Pago), Fmt.Mt(d.Saldo), Fmt.Dt(d.Vencimento), d.Estado);
            if (d.AReceber) l.Com(AcaoVM.Link("Pagar", $"/Caixa/Pagar/{d.Id}", "ti-cash"));
            if (d.Pago < d.Valor) l.Com(AcaoVM.Link("Editar", $"/Caixa/EditarDebito/{d.Id}", "ti-edit", "btn-ghost"));
            vm.Linhas.Add(l);
        }
        return Tabela(vm);
    }

    // ======================= Pagamento (parcial / completo) =======================
    private FormVM FormPagar(Debito d) => new()
    {
        Titulo = "Pagamento de Débito", Subtitulo = d.Numero, ActionUrl = $"/Caixa/Pagar/{d.Id}", VoltarUrl = "/Caixa/ListaDebitos", Botao = "Confirmar pagamento",
        Info = "Escolha «Completo» para liquidar o saldo ou «Parcial» e indique o valor a pagar.",
        Campos =
        {
            CampoVM.Ler("x1", "Cliente", M.NomeCliente(d.ClienteId)),
            CampoVM.Ler("x2", "Descrição", d.Descricao),
            CampoVM.Ler("x3", "Valor do débito", Fmt.Mt(d.Valor)),
            CampoVM.Ler("x4", "Saldo em dívida", Fmt.Mt(d.Saldo)),
            CampoVM.Sel("Tipo", "Tipo de pagamento", Opcoes.De("Completo", "Parcial")),
            CampoVM.Num("Valor", "Valor a pagar (MT) — só para parcial", Fmt.Num(d.Saldo), obrig: false),
            CampoVM.Sel("Metodo", "Método de pagamento", Opcoes.De("Numerário", "Transferência", "Cheque", "M-Pesa", "POS"))
        }
    };

    [HttpGet]
    public IActionResult Pagar(int id)
    {
        var d = M.Deb(id);
        if (d is null || !d.AReceber) return Falha("Débito não disponível para pagamento.", "/Caixa/ListaDebitos");
        return Form(FormPagar(d));
    }

    [HttpPost]
    public IActionResult Pagar(int id, IFormCollection f)
    {
        var d = M.Deb(id);
        if (d is null) return Falha("Débito não encontrado.", "/Caixa/ListaDebitos");
        var r = _svc.Pagar(id, f["Tipo"] == "Completo", Dec(f["Valor"]), f["Metodo"].ToString());
        if (!r.IsSuccess) return FormComErro(FormPagar(d), f, r.Error!);
        return Sucesso($"Recibo {r.Value!.Numero} emitido ({Fmt.Mt(r.Value.Valor)}).", "/Caixa/PagamentosDoDia");
    }

    // ======================= Editar Débito (só campos permitidos) =======================
    private static FormVM FormEditar(Debito d) => new()
    {
        Titulo = "Editar Débito", Subtitulo = d.Numero, ActionUrl = $"/Caixa/EditarDebito/{d.Id}", VoltarUrl = "/Caixa/ListaDebitos",
        Info = "Apenas a descrição e a data de vencimento podem ser alteradas.",
        Campos =
        {
            CampoVM.Ler("x1", "Cliente", M.NomeCliente(d.ClienteId)),
            CampoVM.Ler("x2", "Apólice", M.Apo(d.ApoliceId)?.Numero ?? "—"),
            CampoVM.Ler("x3", "Valor", Fmt.Mt(d.Valor)),
            CampoVM.Ler("x4", "Já pago", Fmt.Mt(d.Pago)),
            CampoVM.Txt("Descricao", "Descrição", larg: 12, valor: d.Descricao),
            CampoVM.Dat("Vencimento", "Vencimento", d.Vencimento)
        }
    };

    [HttpGet]
    public IActionResult EditarDebito(int id)
    {
        var d = M.Deb(id);
        return d is null ? Falha("Débito não encontrado.", "/Caixa/ListaDebitos") : Form(FormEditar(d));
    }

    [HttpPost]
    public IActionResult EditarDebito(int id, IFormCollection f)
    {
        var d = M.Deb(id);
        if (d is null) return Falha("Débito não encontrado.", "/Caixa/ListaDebitos");
        var r = _svc.EditarDebito(id, f["Descricao"].ToString(), Dt(f["Vencimento"], d.Vencimento));
        if (!r.IsSuccess) return FormComErro(FormEditar(d), f, r.Error!);
        return Sucesso("Débito actualizado.", "/Caixa/ListaDebitos");
    }

    // ======================= Anulação de Recibo =======================
    public IActionResult AnularRecibo()
    {
        var vm = new TabelaVM
        {
            Titulo = "Anulação de Recibo", Subtitulo = "Seleccione o recibo a anular",
            Colunas = { "Recibo", "Data", "Cliente", "Débito", "Método", "Valor" }
        };
        foreach (var p in M.Pagamentos.Where(p => !p.Anulado).OrderByDescending(p => p.Data))
        {
            var d = M.Deb(p.DebitoId);
            vm.Linhas.Add(LinhaVM.De(p.Numero, Fmt.Dt(p.Data), M.NomeCliente(d?.ClienteId ?? 0), d?.Numero ?? "—", p.Metodo, Fmt.Mt(p.Valor))
                .Com(AcaoVM.Link("Anular", $"/Caixa/AnularReciboMotivo/{p.Id}", "ti-x", "btn-danger")));
        }
        return Tabela(vm);
    }

    private static FormVM FormMotivo(string titulo, string sub, string action, string voltar) => new()
    {
        Titulo = titulo, Subtitulo = sub, ActionUrl = action, VoltarUrl = voltar, Botao = "Confirmar anulação",
        Campos = { CampoVM.Area("Motivo", "Motivo da anulação") }
    };

    [HttpGet]
    public IActionResult AnularReciboMotivo(int id)
    {
        var p = M.Pagamentos.FirstOrDefault(x => x.Id == id);
        return p is null ? Falha("Recibo não encontrado.", "/Caixa/AnularRecibo")
            : Form(FormMotivo("Anular Recibo", $"{p.Numero} — {Fmt.Mt(p.Valor)}", $"/Caixa/AnularReciboMotivo/{id}", "/Caixa/AnularRecibo"));
    }

    [HttpPost]
    public IActionResult AnularReciboMotivo(int id, IFormCollection f)
    {
        var r = _svc.AnularRecibo(id, f["Motivo"].ToString());
        if (!r.IsSuccess) return FormComErro(FormMotivo("Anular Recibo", "", $"/Caixa/AnularReciboMotivo/{id}", "/Caixa/AnularRecibo"), f, r.Error!);
        return Sucesso($"Recibo {r.Value!.Numero} anulado.", "/Caixa/AnularRecibo");
    }

    // ======================= Anulação da Apólice =======================
    public IActionResult AnularApolice()
    {
        var vm = new TabelaVM
        {
            Titulo = "Anulação da Apólice", Subtitulo = "Seleccione a apólice a anular (sem recibos activos)",
            Colunas = { "Nº Apólice", "Cliente", "Seguradora", "Ramo", "Prémio", "Estado" }
        };
        foreach (var a in M.Apolices.Where(a => a.Estado != "Anulada"))
            vm.Linhas.Add(LinhaVM.De(a.Numero, M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), a.Ramo, Fmt.Mt(a.Premio), a.Estado)
                .Com(AcaoVM.Link("Anular", $"/Caixa/AnularApoliceMotivo/{a.Id}", "ti-x", "btn-danger")));
        return Tabela(vm);
    }

    [HttpGet]
    public IActionResult AnularApoliceMotivo(int id)
    {
        var a = M.Apo(id);
        return a is null ? Falha("Apólice não encontrada.", "/Caixa/AnularApolice")
            : Form(FormMotivo("Anular Apólice", a.Numero, $"/Caixa/AnularApoliceMotivo/{id}", "/Caixa/AnularApolice"));
    }

    [HttpPost]
    public IActionResult AnularApoliceMotivo(int id, IFormCollection f)
    {
        var r = _svc.AnularApolice(id, f["Motivo"].ToString());
        if (!r.IsSuccess) return FormComErro(FormMotivo("Anular Apólice", "", $"/Caixa/AnularApoliceMotivo/{id}", "/Caixa/AnularApolice"), f, r.Error!);
        return Sucesso($"Apólice {r.Value!.Numero} anulada.", "/Caixa/ApolicesAnuladas");
    }

    // ======================= Listas =======================
    public IActionResult PagamentosDoDia()
    {
        var vm = new TabelaVM
        {
            Titulo = "Pagamentos do Dia", Subtitulo = Fmt.Dt(DateTime.Today),
            Colunas = { "Recibo", "Cliente", "Débito", "Método", "Valor", "Tipo", "Contabilizado" }
        };
        var dia = M.Pagamentos.Where(p => !p.Anulado && p.Data.Date == DateTime.Today).ToList();
        foreach (var p in dia)
        {
            var d = M.Deb(p.DebitoId);
            vm.Linhas.Add(LinhaVM.De(p.Numero, M.NomeCliente(d?.ClienteId ?? 0), d?.Numero ?? "—", p.Metodo, Fmt.Mt(p.Valor), p.Parcial ? "Parcial" : "Completo", p.Contabilizado ? "Sim" : "Não"));
        }
        vm.Resumo.Add(("Nº de recibos", dia.Count.ToString()));
        vm.Resumo.Add(("Total do dia", Fmt.Mt(dia.Sum(p => p.Valor))));
        return Tabela(vm);
    }

    public IActionResult ApolicesAnuladas()
    {
        var vm = new TabelaVM
        {
            Titulo = "Apólices Anuladas", Subtitulo = "Histórico de anulações",
            Colunas = { "Nº Apólice", "Cliente", "Seguradora", "Prémio", "Data de anulação", "Motivo" }
        };
        foreach (var a in M.Apolices.Where(a => a.Estado == "Anulada"))
            vm.Linhas.Add(LinhaVM.De(a.Numero, M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), Fmt.Mt(a.Premio), Fmt.Dt(a.DataAnulacao), a.MotivoAnulacao ?? "—"));
        return Tabela(vm);
    }

    public IActionResult PagamentosParciais()
    {
        var vm = new TabelaVM
        {
            Titulo = "Lista de Pagamentos Parciais", Subtitulo = "Débitos com pagamento parcial em aberto",
            Colunas = { "Nº Débito", "Cliente", "Valor", "Pago", "Saldo", "Vencimento", "Estado" }
        };
        foreach (var d in M.Debitos.Where(d => d.AReceber && d.Pago > 0))
            vm.Linhas.Add(LinhaVM.De(d.Numero, M.NomeCliente(d.ClienteId), Fmt.Mt(d.Valor), Fmt.Mt(d.Pago), Fmt.Mt(d.Saldo), Fmt.Dt(d.Vencimento), d.Estado)
                .Com(AcaoVM.Link("Pagar saldo", $"/Caixa/Pagar/{d.Id}", "ti-cash")));
        return Tabela(vm);
    }

    // ======================= Lançar Débitos à Contabilidade =======================
    [HttpGet]
    public IActionResult LancarContabilidade()
    {
        var pend = M.Pagamentos.Where(p => !p.Anulado && !p.Contabilizado).ToList();
        var vm = new TabelaVM
        {
            Titulo = "Lançar Débitos à Contabilidade", Subtitulo = "Pagamentos ainda não contabilizados",
            Colunas = { "Recibo", "Data", "Cliente", "Método", "Valor", "Comissão" },
            Vazio = "Todos os pagamentos já foram lançados."
        };
        foreach (var p in pend)
            vm.Linhas.Add(LinhaVM.De(p.Numero, Fmt.Dt(p.Data), M.NomeCliente(M.Deb(p.DebitoId)?.ClienteId ?? 0), p.Metodo, Fmt.Mt(p.Valor), Fmt.Mt(M.Comissao(p))));
        vm.Resumo.Add(("Por lançar", pend.Count.ToString()));
        vm.Resumo.Add(("Valor total", Fmt.Mt(pend.Sum(p => p.Valor))));
        if (pend.Count > 0) vm.AcoesTopo.Add(AcaoVM.Envio("Lançar à Contabilidade", "/Caixa/LancarContabilidade", $"Lançar {pend.Count} pagamento(s) à contabilidade?", "ti-book"));
        return Tabela(vm);
    }

    [HttpPost, ActionName("LancarContabilidade")]
    public IActionResult LancarContabilidadePost()
    {
        var r = _svc.LancarContabilidade();
        return r.IsSuccess ? Sucesso($"{r.Value} pagamento(s) lançado(s) à contabilidade.", "/Caixa/LancarContabilidade")
                           : Falha(r.Error!, "/Caixa/LancarContabilidade");
    }
}
