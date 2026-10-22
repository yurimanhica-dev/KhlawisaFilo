namespace Klahisa.Web.Features.Seguros;

public class SegurosController : BaseController
{
    private readonly CorretoraService _svc;
    public SegurosController(CorretoraService svc) => _svc = svc;

    // ======================= Pedido de Seguro =======================
    private static CampoVM NaAba(CampoVM campo, string aba)
    {
        campo.Aba = aba;
        return campo;
    }

    private static CampoVM Leitura(CampoVM campo)
    {
        campo.SoLeitura = true;
        return campo;
    }

    private static CampoVM NoCartaoLateral(CampoVM campo)
    {
        campo.CartaoLateral = true;
        return campo;
    }

    private static FormVM FormPedido()
    {
        var tipoSeguro = CampoVM.Sel("TipoSeguroId", "Tipo de seguro", M.Cadastros
            .Where(c => c.Tipo == "tipos-seguro" && c.Estado == "Activo")
            .OrderBy(c => c.Descricao).Select(c => (c.Id.ToString(), c.Descricao)).ToList());
        tipoSeguro.DependeDe = "SeguradoraId";
        foreach (var item in M.Cadastros.Where(c => c.Tipo == "tipos-seguro" && c.Estado == "Activo"))
        {
            tipoSeguro.OpcaoPai[item.Id.ToString()] = item.SeguradoraId.ToString();
            tipoSeguro.DadosOpcao[item.Id.ToString()] = string.Join("|", item.SeguradoraId, item.Extra,
                Fmt.Num(item.Coeficiente), Fmt.Num(item.EncargoPct), Fmt.Num(item.SeloPct), Fmt.Num(item.SobretaxaPct), Fmt.Num(item.ComissaoPct));
        }

        var seguradora = CampoVM.Sel("SeguradoraId", "Seguradora", Opcoes.Seguradoras(), larg: 6);
        foreach (var item in M.Seguradoras) seguradora.DadosOpcao[item.Id.ToString()] = item.ReferenciaPadrao;
        var cliente = CampoVM.Sel("ClienteId", "Nome do cliente", Opcoes.Clientes(), larg: 9);
        cliente.Pesquisavel = true;
        var moeda = CampoVM.Sel("Moeda", "Código da moeda", Opcoes.Moedas(), "MZN", larg: 3);
        moeda.DadosOpcao["MZN"] = "1.00";
        foreach (var item in M.Cadastros.Where(c => c.Tipo == "cambios"))
        {
            if (decimal.TryParse(item.Extra, NumberStyles.Number, CultureInfo.InvariantCulture, out var taxa) && taxa > 0)
                moeda.DadosOpcao[item.Codigo] = Fmt.Num(taxa);
        }
        var campos = new[]
        {
            NaAba(CampoVM.Ler("Referencia", "Referência da apólice", "Gerada ao guardar", 4), "Apólice"),
            NaAba(CampoVM.Txt("NumeroApolice", "Número da apólice", larg: 4, tipo: "text"), "Apólice"),
            NaAba(CampoVM.Dat("DataEmissao", "Data de emissão", DateTime.Today, larg: 4), "Apólice"),
            NaAba(seguradora, "Apólice"),
            NaAba(CampoVM.Txt("ReferenciaSeguradora", "Referência da seguradora", obrig: false, larg: 6), "Apólice"),
            NaAba(cliente, "Apólice"),
            NaAba(CampoVM.Sel("Periodicidade", "Periodicidade", Opcoes.De("Anual", "Semestral", "Trimestral", "Mensal"), larg: 3), "Apólice"),
            NaAba(CampoVM.Dat("Inicio", "Data de início", DateTime.Today, larg: 4), "Apólice"),
            NaAba(CampoVM.Dat("Fim", "Data final", DateTime.Today.AddYears(1).AddDays(-1), larg: 4), "Apólice"),
            NaAba(CampoVM.Dat("DataRenovacao", "Data de renovação", DateTime.Today.AddYears(1), larg: 4), "Apólice"),
            NaAba(moeda, "Apólice"),
            NaAba(CampoVM.Sel("TipoCambio", "Câmbio a usar", Opcoes.De("Venda", "Compra"), "Venda", larg: 3), "Apólice"),
            NaAba(Leitura(CampoVM.Num("Cambio", "Câmbio parametrizado", "1", obrig: false, larg: 3)), "Apólice"),
            NaAba(tipoSeguro, "Detalhes"),
            NaAba(CampoVM.Txt("Objecto", "Descrição", larg: 12), "Detalhes"),
            NaAba(CampoVM.Num("Capital", "Valor segurado (MT)", larg: 3), "Detalhes"),
            NaAba(CampoVM.Num("Premio", "Prémio total anual", obrig: true, larg: 3), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("PremioLiquido", "Prémio líquido", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("ComissaoPct", "% Comissão", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("ComissaoValor", "Comissão", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("EncargoPct", "% Encargos", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("EncargoValor", "Encargos", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("SeloPct", "% Selos", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("SeloValor", "Selos", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("SobretaxaPct", "% Sobretaxa", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("SobretaxaValor", "Sobretaxa", obrig: false, larg: 12))), "Detalhes"),
            NaAba(NoCartaoLateral(Leitura(CampoVM.Num("Coeficiente", "Coeficiente", obrig: false, larg: 12))), "Detalhes")
        };
        var vm = new FormVM
        {
            Titulo = "Pedido de Seguro",
            Subtitulo = "Registar novo pedido / cotação",
            ActionUrl = "/Seguros/Pedido",
            VoltarUrl = "/Seguros/Lista",
            Info = "Seleccione primeiro a seguradora para listar os tipos de seguro associados. As taxas vêm do tipo escolhido. Informe o prémio anual total; o prémio líquido é calculado automaticamente.",
            UsaAbas = true
        };
        vm.Campos.AddRange(campos);
        return vm;
    }

    [HttpGet]
    public IActionResult Pedido() => Form(FormPedido());

    [HttpGet]
    public IActionResult Renovar(int id)
    {
        var apolice = M.Apolices.FirstOrDefault(a => a.Id == id && a.Estado == "Activa");
        if (apolice is null) return Falha("Apólice activa não encontrada para renovação.", "/Seguros/ARenovar");

        var vm = FormPedido();
        vm.Titulo = "Renovar Seguro";
        vm.Subtitulo = $"Renovação da apólice {apolice.Numero}";
        vm.Info = "Confirme os dados e ajuste o prémio ou a data de início antes de registar a renovação.";
        vm.Botao = "Registar renovação";
        var config = M.Cadastros.FirstOrDefault(c => c.Tipo == "tipos-seguro" && c.Descricao == apolice.TipoSeguro);
        foreach (var campo in vm.Campos)
        {
            campo.Valor = campo.Nome switch
            {
                "Referencia" => apolice.Referencia,
                "NumeroApolice" => apolice.NumeroApolice,
                "DataEmissao" => DateTime.Today.ToString("yyyy-MM-dd"),
                "ReferenciaSeguradora" => apolice.ReferenciaSeguradora,
                "ClienteId" => apolice.ClienteId.ToString(),
                "SeguradoraId" => apolice.SeguradoraId.ToString(),
                "Ramo" => apolice.Ramo,
                "TipoSeguroId" => (config?.Id ?? apolice.TipoSeguroId).ToString(),
                "Objecto" => apolice.Objecto,
                "Capital" => Fmt.Num(apolice.Capital),
                "Premio" => Fmt.Num(apolice.Premio),
                "PremioLiquido" => Fmt.Num(apolice.PremioLiquido),
                "ComissaoPct" => Fmt.Num(apolice.ComissaoPct),
                "ComissaoValor" => Fmt.Num(apolice.ComissaoValor),
                "EncargoPct" => Fmt.Num(apolice.EncargoPct),
                "EncargoValor" => Fmt.Num(apolice.EncargoValor),
                "SeloPct" => Fmt.Num(apolice.SeloPct),
                "SeloValor" => Fmt.Num(apolice.SeloValor),
                "SobretaxaPct" => Fmt.Num(apolice.SobretaxaPct),
                "SobretaxaValor" => Fmt.Num(apolice.SobretaxaValor),
                "Coeficiente" => Fmt.Num(apolice.Coeficiente),
                "Inicio" => apolice.Fim.AddDays(1).ToString("yyyy-MM-dd"),
                "Fim" => apolice.Fim.AddDays(1).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd"),
                "DataRenovacao" => apolice.Fim.AddDays(1).AddYears(1).ToString("yyyy-MM-dd"),
                "Periodicidade" => apolice.Periodicidade,
                "Moeda" => apolice.Moeda,
                "TipoCambio" => apolice.TipoCambio,
                "Cambio" => Fmt.Num(apolice.Cambio),
                _ => campo.Valor
            };
        }
        return Form(vm);
    }

    [HttpPost]
    public IActionResult Pedido(IFormCollection f)
    {
        var tipoSeguro = M.Cadastros.FirstOrDefault(c => c.Id == Int(f["TipoSeguroId"])
            && c.Tipo == "tipos-seguro" && c.Estado == "Activo");
        if (tipoSeguro is null)
            return FormComErro(FormPedido(), f, "Seleccione um tipo de seguro activo.");
        var seguradoraId = Int(f["SeguradoraId"]);
        if (M.Seg(seguradoraId) is null || tipoSeguro.SeguradoraId != seguradoraId)
            return FormComErro(FormPedido(), f, "Seleccione um tipo de seguro pertencente à seguradora escolhida.");
        var numeroApolice = f["NumeroApolice"].ToString().Trim();
        if (string.IsNullOrWhiteSpace(numeroApolice))
            return FormComErro(FormPedido(), f, "Informe o número da apólice.");

        var periodos = new Dictionary<string, int> { ["Anual"] = 12, ["Semestral"] = 6, ["Trimestral"] = 3, ["Mensal"] = 1 };
        if (!periodos.TryGetValue(f["Periodicidade"].ToString(), out var meses))
            return FormComErro(FormPedido(), f, "Seleccione uma periodicidade válida.");

        var inicio = Dt(f["Inicio"]);
        var fim = Dt(f["Fim"]);
        var renovacao = Dt(f["DataRenovacao"]);
        var fimEsperado = inicio.AddYears(1).AddDays(-1);
        var renovacaoEsperada = inicio.AddYears(1);
        var capital = Dec(f["Capital"]);
        var moeda = f["Moeda"].ToString();
        var parametroCambio = moeda == "MZN" ? "1" : M.Cadastros.FirstOrDefault(c => c.Tipo == "cambios" && c.Codigo == moeda)?.Extra;
        if (!decimal.TryParse(parametroCambio, NumberStyles.Number, CultureInfo.InvariantCulture, out var cambio) || cambio <= 0)
            return FormComErro(FormPedido(), f, "A moeda seleccionada não tem um câmbio válido parametrizado.");
        var premioTotal = Dec(f["Premio"]);
        if (capital <= 0 || premioTotal <= 0 || fim != fimEsperado || renovacao != renovacaoEsperada)
            return FormComErro(FormPedido(), f, "Verifique o capital, câmbio e datas da apólice.");
        if (string.IsNullOrWhiteSpace(f["Objecto"]))
            return FormComErro(FormPedido(), f, "Informe a descrição do objecto seguro.");
        if (!Opcoes.Moedas().Any(m => m.Item1 == moeda) || f["TipoCambio"].ToString() is not ("Venda" or "Compra"))
            return FormComErro(FormPedido(), f, "Seleccione uma moeda e um tipo de câmbio válidos.");

        var periodosAno = 12 / meses;
        var premioLiquido = Math.Round(premioTotal * tipoSeguro.Coeficiente / periodosAno, 2);
        var encargoValor = Math.Round(premioLiquido * tipoSeguro.EncargoPct / 100m, 2);
        var seloValor = Math.Round(premioLiquido * tipoSeguro.SeloPct / 100m, 2);
        var sobretaxaValor = Math.Round(premioLiquido * tipoSeguro.SobretaxaPct / 100m, 2);
        var comissaoValor = Math.Round(premioLiquido * tipoSeguro.ComissaoPct / 100m, 2);
        var a = new Apolice
        {
            ClienteId = Int(f["ClienteId"]),
            SeguradoraId = seguradoraId,
            TipoSeguroId = tipoSeguro.Id,
            Ramo = tipoSeguro.Extra,
            TipoSeguro = tipoSeguro.Descricao,
            Objecto = f["Objecto"].ToString().Trim(),
            NumeroApolice = numeroApolice,
            ReferenciaSeguradora = f["ReferenciaSeguradora"].ToString().Trim(),
            Capital = capital,
            PremioLiquido = premioLiquido,
            EncargoPct = tipoSeguro.EncargoPct,
            EncargoValor = encargoValor,
            SeloPct = tipoSeguro.SeloPct,
            SeloValor = seloValor,
            SobretaxaPct = tipoSeguro.SobretaxaPct,
            SobretaxaValor = sobretaxaValor,
            ComissaoPct = tipoSeguro.ComissaoPct,
            ComissaoValor = comissaoValor,
            Coeficiente = tipoSeguro.Coeficiente,
            Premio = premioTotal,
            DataEmissao = Dt(f["DataEmissao"]),
            DataRenovacao = renovacao,
            Periodicidade = f["Periodicidade"].ToString(),
            Moeda = moeda,
            TipoCambio = f["TipoCambio"].ToString(),
            Cambio = cambio,
            Inicio = inicio,
            Fim = fim
        };
        var r = _svc.CriarPedido(a, meses);
        if (!r.IsSuccess) return FormComErro(FormPedido(), f, r.Error!);
        return Sucesso($"Pedido {r.Value!.Numero} registado.", "/Seguros/Lista");
    }

    public IActionResult Lista()
    {
        var vm = new TabelaVM
        {
            Titulo = "Listagem de Seguros",
            Subtitulo = "Apólices e pedidos",
            Colunas = { "Referência", "Número da apólice", "Cliente", "Seguradora", "Ramo", "Objecto", "Prémio", "Início", "Fim", "Estado" },
            AcoesTopo = { AcaoVM.Link("Novo pedido", "/Seguros/Pedido", "ti-plus") }
        };
        foreach (var a in M.Apolices.OrderByDescending(a => a.Id))
            vm.Linhas.Add(LinhaVM.De(string.IsNullOrWhiteSpace(a.Referencia) ? a.Numero : a.Referencia,
                string.IsNullOrWhiteSpace(a.NumeroApolice) ? "—" : a.NumeroApolice,
                M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), a.Ramo, a.Objecto,
                Fmt.Mt(a.Premio), Fmt.Dt(a.Inicio), Fmt.Dt(a.Fim), a.Estado));
        return Tabela(vm);
    }

    // ======================= Alocação de Débitos =======================
    public IActionResult AlocacaoDebitos()
    {
        var vm = new TabelaVM
        {
            Titulo = "Alocação de Débitos",
            Subtitulo = "Débitos por emitir — emitir e activar apólices",
            Colunas = { "Nº Débito", "Apólice", "Cliente", "Seguradora", "Valor", "Vencimento" },
            Vazio = "Não há débitos por emitir."
        };
        foreach (var d in M.Debitos.Where(d => !d.Emitido && !d.Anulado))
            vm.Linhas.Add(LinhaVM.De(d.Numero, M.Apo(d.ApoliceId)?.Numero ?? "—", M.NomeCliente(d.ClienteId), M.NomeSeg(d.SeguradoraId),
                Fmt.Mt(d.Valor), Fmt.Dt(d.Vencimento))
                .Com(AcaoVM.Envio("Emitir / Alocar", $"/Seguros/Alocar/{d.Id}", "Emitir este débito e activar a apólice?", "ti-check")));
        return Tabela(vm);
    }

    [HttpGet]
    public IActionResult PrepararAlocacao(int id)
    {
        var debito = M.Deb(id);
        if (debito is null || debito.Emitido || debito.Anulado)
            return Falha("Débito não disponível para emissão.", "/Seguros/AlocacaoDebitos");

        var vm = new FormVM
        {
            Titulo = "Emitir / Alocar Débito",
            Subtitulo = debito.Numero,
            ActionUrl = $"/Seguros/Alocar/{debito.Id}",
            VoltarUrl = "/Seguros/AlocacaoDebitos",
            Botao = "Emitir e activar apólice",
            Info = "Confirme o débito. A emissão activa a apólice correspondente.",
            Campos =
            {
                CampoVM.Ler("cliente", "Cliente", M.NomeCliente(debito.ClienteId)),
                CampoVM.Ler("apolice", "Apólice", M.Apo(debito.ApoliceId)?.Numero ?? "—"),
                CampoVM.Ler("valor", "Valor", Fmt.Mt(debito.Valor)),
                CampoVM.Ler("vencimento", "Vencimento", Fmt.Dt(debito.Vencimento))
            }
        };
        return Form(vm);
    }

    [HttpPost]
    public IActionResult Alocar(int id)
    {
        var r = _svc.AlocarDebito(id);
        return r.IsSuccess ? Sucesso($"Débito {r.Value!.Numero} emitido. Apólice activada.", "/Seguros/AlocacaoDebitos")
                           : Falha(r.Error!, "/Seguros/AlocacaoDebitos");
    }

    // ======================= Prestação de Contas =======================
    public IActionResult PrestacaoContas()
    {
        var vm = new TabelaVM
        {
            Titulo = "Prestação de Contas",
            Subtitulo = "Valores a entregar às seguradoras (recebido − comissão)",
            Colunas = { "Seguradora", "Recebido", "Comissão", "Líquido", "Já entregue", "Saldo a entregar" }
        };
        foreach (var s in M.Seguradoras)
        {
            var x = _svc.SaldoSeguradora(s.Id);
            var linha = LinhaVM.De(s.Sigla, Fmt.Mt(x.Recebido), Fmt.Mt(x.Comissao), Fmt.Mt(x.Liquido), Fmt.Mt(x.Entregue), Fmt.Mt(x.Saldo));
            if (x.Saldo > 0)
                linha.Com(AcaoVM.Envio("Prestar contas", $"/Seguros/PrestarContas/{s.Id}", $"Entregar {Fmt.Mt(x.Saldo)} à {s.Sigla}?", "ti-send"));
            vm.Linhas.Add(linha);
        }
        return Tabela(vm);
    }

    [HttpPost]
    public IActionResult PrestarContas(int id)
    {
        var r = _svc.PrestarContas(id);
        return r.IsSuccess ? Sucesso($"Prestação de contas registada: {Fmt.Mt(r.Value)}.", "/Seguros/PrestacaoContas")
                           : Falha(r.Error!, "/Seguros/PrestacaoContas");
    }

    // ======================= Inclusão e Exclusão =======================
    private static FormVM FormIncExc() => new()
    {
        Titulo = "Inclusão e Exclusão",
        Subtitulo = "Endosso à apólice (inclui/exclui objectos ou pessoas)",
        ActionUrl = "/Seguros/InclusaoExclusao",
        VoltarUrl = "/Seguros/Lista",
        Info = "Inclusão gera uma Nota de Débito (prémio adicional). Exclusão gera uma Nota de Crédito (estorno de prémio).",
        Campos =
        {
            CampoVM.Sel("ApoliceId", "Apólice", Opcoes.Apolices(true)),
            CampoVM.Sel("Operacao", "Operação", Opcoes.De("Inclusão", "Exclusão")),
            CampoVM.Txt("Descricao", "Descrição (o que é incluído/excluído)", larg: 12),
            CampoVM.Num("Valor", "Valor do ajuste (MT)")
        }
    };

    [HttpGet]
    public IActionResult InclusaoExclusao() => Form(FormIncExc());

    [HttpPost]
    public IActionResult InclusaoExclusao(IFormCollection f)
    {
        var tipo = f["Operacao"] == "Exclusão" ? "Nota de Crédito" : "Nota de Débito";
        var r = _svc.EmitirNota(Int(f["ApoliceId"]), tipo, $"{f["Operacao"]}: {f["Descricao"]}", Dec(f["Valor"]));
        if (!r.IsSuccess) return FormComErro(FormIncExc(), f, r.Error!);
        return Sucesso($"{tipo} {r.Value!.Numero} emitida.", "/Seguros/Notas");
    }

    // ======================= Nota de Débito e Crédito =======================
    private static FormVM FormNota() => new()
    {
        Titulo = "Nota de Débito e Crédito",
        Subtitulo = "Emissão manual de notas",
        ActionUrl = "/Seguros/NotaDebitoCredito",
        VoltarUrl = "/Seguros/Notas",
        Botao = "Emitir nota",
        Campos =
        {
            CampoVM.Sel("ApoliceId", "Apólice", Opcoes.Apolices(true)),
            CampoVM.Sel("Tipo", "Tipo de nota", Opcoes.De("Nota de Débito", "Nota de Crédito")),
            CampoVM.Txt("Motivo", "Motivo", larg: 12),
            CampoVM.Num("Valor", "Valor (MT)")
        }
    };

    [HttpGet]
    public IActionResult NotaDebitoCredito() => Form(FormNota());

    [HttpPost]
    public IActionResult NotaDebitoCredito(IFormCollection f)
    {
        var r = _svc.EmitirNota(Int(f["ApoliceId"]), f["Tipo"].ToString(), f["Motivo"].ToString(), Dec(f["Valor"]));
        if (!r.IsSuccess) return FormComErro(FormNota(), f, r.Error!);
        return Sucesso($"{r.Value!.Tipo} {r.Value.Numero} emitida.", "/Seguros/Notas");
    }

    public IActionResult Notas()
    {
        var vm = new TabelaVM
        {
            Titulo = "Notas de Débito e Crédito",
            Subtitulo = "Notas emitidas",
            Colunas = { "Nº Nota", "Tipo", "Apólice", "Cliente", "Descrição", "Valor", "Estado" },
            AcoesTopo = { AcaoVM.Link("Nova nota", "/Seguros/NotaDebitoCredito", "ti-plus") }
        };
        foreach (var d in M.Debitos.Where(d => d.Tipo != "Prémio").OrderByDescending(d => d.Id))
            vm.Linhas.Add(LinhaVM.De(d.Numero, d.Tipo, M.Apo(d.ApoliceId)?.Numero ?? "—", M.NomeCliente(d.ClienteId), d.Descricao, Fmt.Mt(d.Valor), d.Estado));
        return Tabela(vm);
    }

    // ======================= Relatórios de Seguros =======================
    private static TabelaVM TabelaDebitos(string titulo, string sub, IEnumerable<Debito> dados)
    {
        var vm = new TabelaVM { Titulo = titulo, Subtitulo = sub, Colunas = { "Nº Débito", "Cliente", "Seguradora", "Apólice", "Valor", "Pago", "Saldo", "Vencimento", "Estado" } };
        foreach (var d in dados)
            vm.Linhas.Add(LinhaVM.De(d.Numero, M.NomeCliente(d.ClienteId), M.NomeSeg(d.SeguradoraId), M.Apo(d.ApoliceId)?.Numero ?? "—",
                Fmt.Mt(d.Valor), Fmt.Mt(d.Pago), Fmt.Mt(d.Saldo), Fmt.Dt(d.Vencimento), d.Estado));
        vm.Resumo.Add(("Registos", vm.Linhas.Count.ToString()));
        vm.Resumo.Add(("Total em saldo", Fmt.Mt(dados.Sum(d => d.Saldo))));
        return vm;
    }

    public IActionResult ARenovar()
    {
        var h = DateTime.Today;
        var vm = new TabelaVM
        {
            Titulo = "Seguros por Renovar",
            Subtitulo = "Apólices activas a terminar nos próximos 60 dias",
            Colunas = { "Nº Apólice", "Cliente", "Seguradora", "Ramo", "Prémio", "Fim", "Dias restantes" }
        };
        foreach (var a in M.Apolices.Where(a => a.Estado == "Activa" && a.Fim >= h && a.Fim <= h.AddDays(60)).OrderBy(a => a.Fim))
            vm.Linhas.Add(LinhaVM.De(a.Numero, M.NomeCliente(a.ClienteId), M.NomeSeg(a.SeguradoraId), a.Ramo, Fmt.Mt(a.Premio), Fmt.Dt(a.Fim), (a.Fim - h).Days.ToString()));
        return Tabela(vm);
    }

    public IActionResult DebitosPorVencer() =>
        Tabela(TabelaDebitos("Débitos por Vencer", "Débitos emitidos ainda dentro do prazo", M.Debitos.Where(d => d.AReceber && d.Vencimento.Date >= DateTime.Today).OrderBy(d => d.Vencimento)));

    public IActionResult DebitosVencidos() =>
        Tabela(TabelaDebitos("Débitos Vencidos", "Débitos em atraso", M.Debitos.Where(d => d.AReceber && d.Vencimento.Date < DateTime.Today).OrderBy(d => d.Vencimento)));

    public IActionResult DebitosPagos() =>
        Tabela(TabelaDebitos("Débitos Pagos", "Débitos totalmente liquidados", M.Debitos.Where(d => d.Estado == "Pago").OrderByDescending(d => d.Vencimento)));

    public IActionResult ExtractoCliente(int? clienteId)
    {
        var vm = new TabelaVM
        {
            Titulo = "Extracto do Cliente",
            Subtitulo = "Movimentos de débito e crédito",
            Pesquisavel = false,
            Vazio = "Seleccione um cliente.",
            Colunas = { "Data", "Documento", "Descrição", "Débito", "Crédito", "Saldo" },
            Filtro = new FiltroVM { Nome = "clienteId", Rotulo = "Cliente", Url = "/Seguros/ExtractoCliente", Valor = clienteId?.ToString() ?? "", Opcoes = Opcoes.Clientes() }
        };
        if (clienteId is null) return Tabela(vm);

        var movs = new List<(DateTime Data, string Doc, string Desc, decimal D, decimal C)>();
        foreach (var d in M.Debitos.Where(d => d.ClienteId == clienteId && d.Emitido && !d.Anulado))
            movs.Add(d.Tipo == "Nota de Crédito" ? (d.Emissao, d.Numero, d.Descricao, 0m, d.Valor) : (d.Emissao, d.Numero, d.Descricao, d.Valor, 0m));
        foreach (var p in M.Pagamentos.Where(p => !p.Anulado && M.Deb(p.DebitoId)?.ClienteId == clienteId))
            movs.Add((p.Data, p.Numero, $"Pagamento — {p.Metodo}", 0m, p.Valor));

        decimal saldo = 0;
        foreach (var m in movs.OrderBy(m => m.Data))
        {
            saldo += m.D - m.C;
            vm.Linhas.Add(LinhaVM.De(Fmt.Dt(m.Data), m.Doc, m.Desc, m.D > 0 ? Fmt.Mt(m.D) : "", m.C > 0 ? Fmt.Mt(m.C) : "", Fmt.Mt(saldo)));
        }
        vm.Resumo.Add(("Cliente", M.NomeCliente(clienteId.Value)));
        vm.Resumo.Add(("Saldo em dívida", Fmt.Mt(saldo)));
        return Tabela(vm);
    }

    public IActionResult ListaPagamentos()
    {
        var vm = new TabelaVM
        {
            Titulo = "Lista de Pagamentos",
            Subtitulo = "Todos os recibos emitidos",
            Colunas = { "Recibo", "Data", "Cliente", "Débito", "Método", "Valor", "Tipo", "Estado" }
        };
        foreach (var p in M.Pagamentos.OrderByDescending(p => p.Data).ThenByDescending(p => p.Id))
        {
            var d = M.Deb(p.DebitoId);
            vm.Linhas.Add(LinhaVM.De(p.Numero, Fmt.Dt(p.Data), M.NomeCliente(d?.ClienteId ?? 0), d?.Numero ?? "—", p.Metodo, Fmt.Mt(p.Valor),
                p.Parcial ? "Parcial" : "Completo", p.Anulado ? "Anulado" : "Pago"));
        }
        vm.Resumo.Add(("Total recebido (válidos)", Fmt.Mt(M.Pagamentos.Where(p => !p.Anulado).Sum(p => p.Valor))));
        return Tabela(vm);
    }

    public IActionResult SaldoSeguradoras()
    {
        var vm = new TabelaVM
        {
            Titulo = "Saldo das Seguradoras",
            Subtitulo = "Posição por seguradora",
            Colunas = { "Seguradora", "Prémios recebidos", "Comissão", "Líquido", "Entregue", "Saldo" },
            Pesquisavel = false
        };
        foreach (var s in M.Seguradoras)
        {
            var x = _svc.SaldoSeguradora(s.Id);
            vm.Linhas.Add(LinhaVM.De(s.Nome, Fmt.Mt(x.Recebido), Fmt.Mt(x.Comissao), Fmt.Mt(x.Liquido), Fmt.Mt(x.Entregue), Fmt.Mt(x.Saldo)));
        }
        return Tabela(vm);
    }
}
