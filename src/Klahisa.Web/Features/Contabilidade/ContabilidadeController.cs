namespace Klahisa.Web.Features.Contabilidade;

public class ContabilidadeController : BaseController
{
    private readonly CorretoraService _svc;
    public ContabilidadeController(CorretoraService svc) => _svc = svc;

    private static readonly string[] TiposConta = { "Activo", "Passivo", "Capital Próprio", "Proveito", "Custo" };

    // ======================= Conta Plano =======================
    public IActionResult ContaPlano()
    {
        var vm = new TabelaVM
        {
            Titulo = "Conta Plano", Subtitulo = "Plano de contas",
            Colunas = { "Código", "Nome", "Tipo", "Aceita lançamentos", "Saldo (D − C)" },
            AcoesTopo = { AcaoVM.Link("Nova conta", "/Contabilidade/NovaConta", "ti-plus") }
        };
        foreach (var c in M.Contas.OrderBy(c => c.Codigo))
        {
            var m = _svc.MovimentosConta(c.Codigo);
            vm.Linhas.Add(LinhaVM.De(c.Codigo, c.Nome, c.Tipo, c.AceitaLancamentos ? "Sim" : "Não", Fmt.Mt(m.Debito - m.Credito))
                .Com(AcaoVM.Link("Editar", $"/Contabilidade/EditarConta/{c.Id}", "ti-edit", "btn-ghost")));
        }
        return Tabela(vm);
    }

    private static FormVM FormConta(ContaPlano? c) => new()
    {
        Titulo = c is null ? "Nova Conta" : "Editar Conta", Subtitulo = "Plano de contas",
        ActionUrl = c is null ? "/Contabilidade/NovaConta" : $"/Contabilidade/EditarConta/{c.Id}", VoltarUrl = "/Contabilidade/ContaPlano",
        Campos =
        {
            c is null ? CampoVM.Txt("Codigo", "Código") : CampoVM.Ler("Codigo", "Código", c.Codigo),
            CampoVM.Txt("Nome", "Nome da conta", valor: c?.Nome ?? ""),
            CampoVM.Sel("Tipo", "Tipo", Opcoes.De(TiposConta), c?.Tipo ?? "Activo"),
            CampoVM.Sel("Aceita", "Aceita lançamentos?", Opcoes.De("Sim", "Não"), c is { AceitaLancamentos: false } ? "Não" : "Sim")
        }
    };

    [HttpGet] public IActionResult NovaConta() => Form(FormConta(null));

    [HttpPost]
    public IActionResult NovaConta(IFormCollection f)
    {
        var r = _svc.GuardarConta(new ContaPlano { Codigo = f["Codigo"].ToString().Trim(), Nome = f["Nome"].ToString(), Tipo = f["Tipo"].ToString(), AceitaLancamentos = f["Aceita"] == "Sim" }, true);
        if (!r.IsSuccess) return FormComErro(FormConta(null), f, r.Error!);
        return Sucesso("Conta criada.", "/Contabilidade/ContaPlano");
    }

    [HttpGet]
    public IActionResult EditarConta(int id)
    {
        var c = M.Contas.FirstOrDefault(x => x.Id == id);
        return c is null ? Falha("Conta não encontrada.", "/Contabilidade/ContaPlano") : Form(FormConta(c));
    }

    [HttpPost]
    public IActionResult EditarConta(int id, IFormCollection f)
    {
        var c = M.Contas.FirstOrDefault(x => x.Id == id);
        if (c is null) return Falha("Conta não encontrada.", "/Contabilidade/ContaPlano");
        var r = _svc.GuardarConta(new ContaPlano { Id = id, Codigo = c.Codigo, Nome = f["Nome"].ToString(), Tipo = f["Tipo"].ToString(), AceitaLancamentos = f["Aceita"] == "Sim" }, false);
        if (!r.IsSuccess) return FormComErro(FormConta(c), f, r.Error!);
        return Sucesso("Conta actualizada.", "/Contabilidade/ContaPlano");
    }

    // ======================= Lançamentos (MZN e moeda estrangeira) =======================
    private static FormVM FormLancamento(bool me)
    {
        var vm = new FormVM
        {
            Titulo = me ? "Lançamento em Moeda Estrangeira" : "Lançamentos Contabilísticos",
            Subtitulo = me ? "Valores na moeda indicada — convertidos para MT ao câmbio informado" : "Novo lançamento em Meticais",
            ActionUrl = me ? "/Contabilidade/LancamentoME" : "/Contabilidade/Lancamentos", VoltarUrl = "/Contabilidade/Diario",
            Info = "Preencha pelo menos duas linhas. O total a débito deve ser igual ao total a crédito.",
            Campos = { CampoVM.Dat("Data", "Data", DateTime.Today), CampoVM.Txt("Descricao", "Descrição") }
        };
        if (me)
        {
            vm.Campos.Add(CampoVM.Sel("Moeda", "Moeda", Opcoes.Moedas().Where(m => m.Item1 != "MZN").ToList()));
            vm.Campos.Add(CampoVM.Num("Cambio", "Taxa de câmbio (MT por unidade)"));
        }
        for (var i = 1; i <= 4; i++)
        {
            vm.Campos.Add(CampoVM.Sel($"Conta{i}", $"Conta (linha {i})", Opcoes.Contas(true), obrig: i <= 2, larg: 6));
            vm.Campos.Add(CampoVM.Num($"Debito{i}", "Débito", obrig: false, larg: 3));
            vm.Campos.Add(CampoVM.Num($"Credito{i}", "Crédito", obrig: false, larg: 3));
        }
        return vm;
    }

    [HttpGet] public IActionResult Lancamentos() => Form(FormLancamento(false));
    [HttpGet] public IActionResult LancamentoME() => Form(FormLancamento(true));

    [HttpPost]
    public IActionResult Lancamentos(IFormCollection f) => Gravar(f, false);

    [HttpPost]
    public IActionResult LancamentoME(IFormCollection f) => Gravar(f, true);

    private IActionResult Gravar(IFormCollection f, bool me)
    {
        var linhas = new List<(string Conta, decimal D, decimal C)>();
        for (var i = 1; i <= 4; i++) linhas.Add((f[$"Conta{i}"].ToString(), Dec(f[$"Debito{i}"]), Dec(f[$"Credito{i}"])));
        var moeda = me ? f["Moeda"].ToString() : "MZN";
        var r = _svc.Lancar(Dt(f["Data"]), f["Descricao"].ToString(), moeda, me ? Dec(f["Cambio"]) : 1, linhas);
        if (!r.IsSuccess) return FormComErro(FormLancamento(me), f, r.Error!);
        return Sucesso($"Lançamento {r.Value!.Numero} registado.", "/Contabilidade/Diario");
    }

    // ======================= Diário / Reimpressão =======================
    public IActionResult Diario()
    {
        var vm = new TabelaVM
        {
            Titulo = "Diário de Lançamentos", Subtitulo = "Todos os lançamentos por data",
            Colunas = { "Nº", "Data", "Descrição", "Conta", "Débito", "Crédito", "Origem" },
            AcoesTopo = { AcaoVM.Link("Novo lançamento", "/Contabilidade/Lancamentos", "ti-plus") }
        };
        foreach (var l in M.Lancamentos.OrderBy(l => l.Data).ThenBy(l => l.Id))
        {
            vm.Linhas.Add(LinhaVM.De("## " + l.Numero + " — " + Fmt.Dt(l.Data) + " — " + l.Descricao));
            foreach (var x in l.Linhas)
                vm.Linhas.Add(LinhaVM.De(l.Numero, Fmt.Dt(l.Data), l.Descricao, $"{x.Conta} — {M.Contas.FirstOrDefault(c => c.Codigo == x.Conta)?.Nome}",
                    x.Debito > 0 ? Fmt.Mt(x.Debito) : "", x.Credito > 0 ? Fmt.Mt(x.Credito) : "", l.Origem));
        }
        vm.Resumo.Add(("Total a débito", Fmt.Mt(M.Lancamentos.Sum(l => l.Linhas.Sum(x => x.Debito)))));
        vm.Resumo.Add(("Total a crédito", Fmt.Mt(M.Lancamentos.Sum(l => l.Linhas.Sum(x => x.Credito)))));
        return Tabela(vm);
    }

    public IActionResult Reimpressao()
    {
        var vm = new TabelaVM
        {
            Titulo = "Reimpressão de Lançamentos", Subtitulo = "Seleccione o lançamento a reimprimir",
            Colunas = { "Nº", "Data", "Descrição", "Moeda", "Total", "Origem" }
        };
        foreach (var l in M.Lancamentos.OrderByDescending(l => l.Data).ThenByDescending(l => l.Id))
            vm.Linhas.Add(LinhaVM.De(l.Numero, Fmt.Dt(l.Data), l.Descricao, l.Moeda, Fmt.Mt(l.Linhas.Sum(x => x.Debito)), l.Origem)
                .Com(AcaoVM.Link("Reimprimir", $"/Contabilidade/Lancamento/{l.Id}", "ti-printer", "btn-ghost")));
        return Tabela(vm);
    }

    public IActionResult Lancamento(int id)
    {
        var l = M.Lancamentos.FirstOrDefault(x => x.Id == id);
        if (l is null) return Falha("Lançamento não encontrado.", "/Contabilidade/Reimpressao");
        var vm = new TabelaVM
        {
            Titulo = $"Lançamento {l.Numero}", Subtitulo = $"{Fmt.Dt(l.Data)} — {l.Descricao}", Pesquisavel = false,
            Colunas = { "Conta", "Nome", "Débito", "Crédito" },
            AcoesTopo = { AcaoVM.Link("Imprimir", "javascript:window.print()", "ti-printer"), AcaoVM.Link("Voltar", "/Contabilidade/Reimpressao", "ti-arrow-left", "btn-ghost") }
        };
        foreach (var x in l.Linhas)
            vm.Linhas.Add(LinhaVM.De(x.Conta, M.Contas.FirstOrDefault(c => c.Codigo == x.Conta)?.Nome ?? "—", x.Debito > 0 ? Fmt.Mt(x.Debito) : "", x.Credito > 0 ? Fmt.Mt(x.Credito) : ""));
        vm.Resumo.Add(("Origem", l.Origem));
        vm.Resumo.Add(("Moeda / câmbio", $"{l.Moeda} / {l.Cambio:0.00}"));
        return Tabela(vm);
    }

    // ======================= Extracto da Conta Razão =======================
    public IActionResult ExtractoRazao(string? conta)
    {
        var vm = new TabelaVM
        {
            Titulo = "Extracto da Conta Razão", Subtitulo = "Movimentos de uma conta", Pesquisavel = false, Vazio = "Seleccione uma conta.",
            Colunas = { "Data", "Lançamento", "Descrição", "Débito", "Crédito", "Saldo" },
            Filtro = new FiltroVM { Nome = "conta", Rotulo = "Conta", Url = "/Contabilidade/ExtractoRazao", Valor = conta ?? "", Opcoes = Opcoes.Contas(false, false) }
        };
        if (string.IsNullOrEmpty(conta)) return Tabela(vm);

        decimal saldo = 0;
        foreach (var l in M.Lancamentos.OrderBy(l => l.Data).ThenBy(l => l.Id))
            foreach (var x in l.Linhas.Where(x => x.Conta == conta))
            {
                saldo += x.Debito - x.Credito;
                vm.Linhas.Add(LinhaVM.De(Fmt.Dt(l.Data), l.Numero, l.Descricao, x.Debito > 0 ? Fmt.Mt(x.Debito) : "", x.Credito > 0 ? Fmt.Mt(x.Credito) : "", Fmt.Mt(saldo)));
            }
        vm.Resumo.Add(("Conta", $"{conta} — {M.Contas.FirstOrDefault(c => c.Codigo == conta)?.Nome}"));
        vm.Resumo.Add(("Saldo final (D − C)", Fmt.Mt(saldo)));
        return Tabela(vm);
    }

    // ======================= Relatórios =======================
    private decimal Natural(ContaPlano c)
    {
        var m = _svc.MovimentosConta(c.Codigo);
        return c.Tipo is "Activo" or "Custo" ? m.Debito - m.Credito : m.Credito - m.Debito;
    }

    public IActionResult Balancete()
    {
        var vm = new TabelaVM
        {
            Titulo = "Balancete", Subtitulo = "Totais de débito, crédito e saldos por conta", Pesquisavel = false,
            Colunas = { "Conta", "Nome", "Débito", "Crédito", "Saldo devedor", "Saldo credor" }
        };
        decimal td = 0, tc = 0, sd = 0, sc = 0;
        foreach (var c in M.Contas.OrderBy(c => c.Codigo))
        {
            var m = _svc.MovimentosConta(c.Codigo);
            if (m.Debito == 0 && m.Credito == 0) continue;
            var s = m.Debito - m.Credito;
            td += m.Debito; tc += m.Credito;
            if (s > 0) sd += s; else sc += -s;
            vm.Linhas.Add(LinhaVM.De(c.Codigo, c.Nome, Fmt.Mt(m.Debito), Fmt.Mt(m.Credito), s > 0 ? Fmt.Mt(s) : "", s < 0 ? Fmt.Mt(-s) : ""));
        }
        vm.Linhas.Add(LinhaVM.De("## TOTAIS — Débito " + Fmt.Mt(td) + " | Crédito " + Fmt.Mt(tc) + " | Saldos devedores " + Fmt.Mt(sd) + " | Saldos credores " + Fmt.Mt(sc)));
        vm.Resumo.Add(("Equilibrado", td == tc ? "Sim" : "Não"));
        return Tabela(vm);
    }

    private (decimal Proveitos, decimal Custos) Resultado() =>
        (M.Contas.Where(c => c.Tipo == "Proveito").Sum(c => Natural(c)), M.Contas.Where(c => c.Tipo == "Custo").Sum(c => Natural(c)));

    public IActionResult Balanco()
    {
        var vm = new TabelaVM { Titulo = "Balanço", Subtitulo = "Posição financeira", Pesquisavel = false, Colunas = { "Rubrica", "Valor" } };
        var (prov, cust) = Resultado();

        void Secao(string titulo, string tipo, out decimal total)
        {
            vm.Linhas.Add(LinhaVM.De("## " + titulo));
            total = 0;
            foreach (var c in M.Contas.Where(c => c.Tipo == tipo && c.AceitaLancamentos).OrderBy(c => c.Codigo))
            {
                var v = Natural(c);
                if (v == 0) continue;
                total += v;
                vm.Linhas.Add(LinhaVM.De($"{c.Codigo} — {c.Nome}", Fmt.Mt(v)));
            }
            vm.Linhas.Add(LinhaVM.De("Total " + titulo.ToLower(), Fmt.Mt(total)));
        }

        Secao("ACTIVO", "Activo", out var activo);
        Secao("PASSIVO", "Passivo", out var passivo);
        Secao("CAPITAL PRÓPRIO", "Capital Próprio", out var capital);
        vm.Linhas.Add(LinhaVM.De("Resultado do exercício (ainda não apurado)", Fmt.Mt(prov - cust)));
        var totalCp = capital + (prov - cust);
        vm.Resumo.Add(("Activo", Fmt.Mt(activo)));
        vm.Resumo.Add(("Passivo + Capital Próprio", Fmt.Mt(passivo + totalCp)));
        vm.Resumo.Add(("Equilibrado", activo == passivo + totalCp ? "Sim" : "Não"));
        return Tabela(vm);
    }

    public IActionResult DemonstracaoResultados()
    {
        var vm = new TabelaVM { Titulo = "Demonstração de Resultados", Subtitulo = "Proveitos e custos do exercício", Pesquisavel = false, Colunas = { "Rubrica", "Valor" } };
        vm.Linhas.Add(LinhaVM.De("## PROVEITOS"));
        foreach (var c in M.Contas.Where(c => c.Tipo == "Proveito").OrderBy(c => c.Codigo)) vm.Linhas.Add(LinhaVM.De($"{c.Codigo} — {c.Nome}", Fmt.Mt(Natural(c))));
        vm.Linhas.Add(LinhaVM.De("## CUSTOS"));
        foreach (var c in M.Contas.Where(c => c.Tipo == "Custo").OrderBy(c => c.Codigo)) vm.Linhas.Add(LinhaVM.De($"{c.Codigo} — {c.Nome}", Fmt.Mt(Natural(c))));
        var (p, k) = Resultado();
        vm.Resumo.Add(("Total de proveitos", Fmt.Mt(p)));
        vm.Resumo.Add(("Total de custos", Fmt.Mt(k)));
        vm.Resumo.Add(("Resultado líquido", Fmt.Mt(p - k)));
        return Tabela(vm);
    }

    [HttpGet]
    public IActionResult ApuramentoResultados()
    {
        var (p, k) = Resultado();
        var vm = new TabelaVM
        {
            Titulo = "Apuramento de Resultados", Subtitulo = "Transfere proveitos e custos para a conta 81", Pesquisavel = false,
            Colunas = { "Descrição", "Valor" },
            Vazio = "Sem saldos para apurar."
        };
        vm.Linhas.Add(LinhaVM.De("Proveitos por apurar", Fmt.Mt(p)));
        vm.Linhas.Add(LinhaVM.De("Custos por apurar", Fmt.Mt(k)));
        vm.Linhas.Add(LinhaVM.De("Resultado a apurar", Fmt.Mt(p - k)));
        var apurado = M.Lancamentos.Where(l => l.Origem == "Apuramento").Sum(l => l.Linhas.Where(x => x.Conta == "81").Sum(x => x.Credito - x.Debito));
        vm.Resumo.Add(("Resultado já apurado (conta 81)", Fmt.Mt(apurado)));
        vm.AcoesTopo.Add(AcaoVM.Envio("Apurar resultados", "/Contabilidade/ApuramentoResultados", "Apurar os resultados do exercício?", "ti-calculator"));
        return Tabela(vm);
    }

    [HttpPost, ActionName("ApuramentoResultados")]
    public IActionResult ApuramentoResultadosPost()
    {
        var r = _svc.ApurarResultados();
        return r.IsSuccess ? Sucesso("Resultados apurados e transferidos para a conta 81.", "/Contabilidade/ApuramentoResultados")
                           : Falha(r.Error!, "/Contabilidade/ApuramentoResultados");
    }
}
