namespace Klahisa.Web.Features.Configuracao;

public class ConfiguracaoController : BaseController
{
    private readonly CorretoraService _svc;
    public ConfiguracaoController(CorretoraService svc) => _svc = svc;

    // tipo -> (título, rótulo do campo extra)
    private static readonly Dictionary<string, (string Titulo, string Extra)> Tipos = new()
    {
        ["bancos"] = ("Bancos", "SWIFT / BIC"),
        ["paises"] = ("Países", "País"),
        ["provincias"] = ("Províncias", "País"),
        ["distritos"] = ("Distritos", "Província"),
        ["nacionalidades"] = ("Nacionalidades", "Observação"),
        ["profissoes"] = ("Profissões", "Observação"),
        ["entidades-empregadoras"] = ("Entidades empregadoras", "Observação"),
        ["tipos-cliente"] = ("Tipos de Cliente", "Observação"),
        ["tipos-contacto"] = ("Tipos de Contacto", "Observação"),
        ["tipos-documento"] = ("Tipos de Documento de Identificação", "Observação"),
        ["cambios"] = ("Câmbios", "Taxa (MT por unidade)"),
        ["contas-bancarias"] = ("Contas Bancárias", "NIB"),
        ["ramos"] = ("Ramos", "Observação"),
        ["tipos-seguro"] = ("Tipos de Seguros", "Ramo"),
        ["coberturas"] = ("Coberturas", "Ramo"),
        ["balcoes"] = ("Balcões das Seguradoras", "Seguradora"),
        ["selos"] = ("Selos", "Valor (MT)"),
        ["comissoes"] = ("Comissões", "Percentagem (%)")
    };

    public IActionResult Cadastro(string tipo)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        if (tipo == "tipos-seguro") return TabelaTiposSeguro(t);
        if (tipo is "paises" or "provincias" or "distritos") return TabelaLocalidade(tipo);
        var vm = new TabelaVM
        {
            Titulo = t.Titulo, Subtitulo = "Parametrização",
            Colunas = { "Código", "Descrição", t.Extra },
            AcoesTopo = { AcaoVM.Link("Novo registo", $"/Configuracao/CadastroNovo?tipo={tipo}", "ti-plus") }
        };
        foreach (var i in M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Codigo))
            vm.Linhas.Add(LinhaVM.De(i.Codigo, i.Descricao, i.Extra)
                .Com(AcaoVM.Link("Editar", $"/Configuracao/CadastroEditar/{i.Id}?tipo={tipo}", "ti-edit", "btn-ghost"),
                     AcaoVM.Envio("Eliminar", $"/Configuracao/CadastroEliminar/{i.Id}?tipo={tipo}", "Eliminar este registo?", "ti-trash", "btn-danger")));
        return Tabela(vm);
    }

    private IActionResult TabelaTiposSeguro((string Titulo, string Extra) t)
    {
        var vm = new TabelaVM
        {
            Titulo = t.Titulo,
            Subtitulo = "Tipos de seguro, seguradora e taxas aplicáveis",
            Colunas = { "Código", "Tipo de seguro", "Ramo", "Seguradora", "Coeficiente", "% Encargos", "% Selo", "% Sobretaxa", "% Comissão", "Estado" },
            AcoesTopo = { AcaoVM.Link("Novo tipo de seguro", "/Configuracao/CadastroNovo?tipo=tipos-seguro", "ti-plus") }
        };
        foreach (var item in M.Cadastros.Where(c => c.Tipo == "tipos-seguro").OrderBy(c => c.Descricao))
            vm.Linhas.Add(LinhaVM.De(item.Codigo, item.Descricao, item.Extra, M.Seg(item.SeguradoraId)?.Sigla ?? "—",
                Fmt.Num(item.Coeficiente), Fmt.Num(item.EncargoPct), Fmt.Num(item.SeloPct), Fmt.Num(item.SobretaxaPct), Fmt.Num(item.ComissaoPct), item.Estado)
                .Com(AcaoVM.Link("Editar", $"/Configuracao/CadastroEditar/{item.Id}?tipo=tipos-seguro", "ti-edit", "btn-ghost"),
                     AcaoVM.Envio("Eliminar", $"/Configuracao/CadastroEliminar/{item.Id}?tipo=tipos-seguro", "Eliminar este tipo de seguro?", "ti-trash", "btn-danger")));
        return Tabela(vm);
    }

    private static FormVM FormTipoSeguro(CadastroItem? item = null) => new()
    {
        Titulo = item is null ? "Novo tipo de seguro" : "Editar tipo de seguro",
        Subtitulo = "Defina o ramo, a seguradora e os encargos usados nos pedidos",
        ActionUrl = item is null ? "/Configuracao/CadastroNovo?tipo=tipos-seguro" : $"/Configuracao/CadastroEditar/{item.Id}?tipo=tipos-seguro",
        VoltarUrl = "/Configuracao/Cadastro?tipo=tipos-seguro",
        Campos =
        {
            CampoVM.Txt("Codigo", "Código", valor: item?.Codigo ?? "", larg: 3),
            CampoVM.Txt("Descricao", "Tipo de seguro", valor: item?.Descricao ?? "", larg: 6),
            CampoVM.Sel("Ramo", "Ramo", M.Cadastros.Where(c => c.Tipo == "ramos").OrderBy(c => c.Descricao).Select(c => (c.Descricao, c.Descricao)).ToList(), item?.Extra ?? "", larg: 3),
            CampoVM.Sel("SeguradoraId", "Seguradora", Opcoes.Seguradoras(), item?.SeguradoraId.ToString() ?? "", larg: 6),
            CampoVM.Num("Coeficiente", "Coeficiente", Fmt.Num(item?.Coeficiente ?? 0), larg: 3),
            CampoVM.Num("EncargoPct", "% Encargos administrativos", Fmt.Num(item?.EncargoPct ?? 0), larg: 3),
            CampoVM.Num("SeloPct", "% Imposto de selo", Fmt.Num(item?.SeloPct ?? 0), larg: 3),
            CampoVM.Num("SobretaxaPct", "% Sobretaxa", Fmt.Num(item?.SobretaxaPct ?? 0), larg: 3),
            CampoVM.Num("ComissaoPct", "% Comissão", Fmt.Num(item?.ComissaoPct ?? 0), larg: 3),
            CampoVM.Sel("Estado", "Estado", Opcoes.De("Activo", "Inactivo"), item?.Estado ?? "Activo", larg: 3)
        }
    };

    private static bool TryTipoSeguro(IFormCollection f, out CadastroItem item, out string erro)
    {
        item = new CadastroItem { Tipo = "tipos-seguro" };
        erro = "";
        var ramo = f["Ramo"].ToString();
        var seguradoraId = Int(f["SeguradoraId"]);
        if (!M.Cadastros.Any(c => c.Tipo == "ramos" && c.Descricao == ramo) || M.Seg(seguradoraId) is null)
        {
            erro = "Seleccione um ramo e uma seguradora válidos.";
            return false;
        }

        if (!decimal.TryParse(f["Coeficiente"], NumberStyles.Number, CultureInfo.InvariantCulture, out var coeficiente) || coeficiente <= 0
            || !decimal.TryParse(f["EncargoPct"], NumberStyles.Number, CultureInfo.InvariantCulture, out var encargo)
            || !decimal.TryParse(f["SeloPct"], NumberStyles.Number, CultureInfo.InvariantCulture, out var selo)
            || !decimal.TryParse(f["SobretaxaPct"], NumberStyles.Number, CultureInfo.InvariantCulture, out var sobretaxa)
            || !decimal.TryParse(f["ComissaoPct"], NumberStyles.Number, CultureInfo.InvariantCulture, out var comissao)
            || new[] { encargo, selo, sobretaxa, comissao }.Any(p => p < 0 || p > 100))
        {
            erro = "Informe um coeficiente positivo e percentagens entre 0 e 100.";
            return false;
        }

        var estado = f["Estado"].ToString();
        if (estado is not ("Activo" or "Inactivo"))
        {
            erro = "Seleccione um estado válido.";
            return false;
        }

        item.Codigo = f["Codigo"].ToString().Trim();
        item.Descricao = f["Descricao"].ToString().Trim();
        item.Extra = ramo;
        item.SeguradoraId = seguradoraId;
        item.Coeficiente = coeficiente;
        item.EncargoPct = encargo;
        item.SeloPct = selo;
        item.SobretaxaPct = sobretaxa;
        item.ComissaoPct = comissao;
        item.Estado = estado;
        return true;
    }

    private IActionResult TabelaLocalidade(string tipo)
    {
        var t = Tipos[tipo];
        var paiTipo = TipoPai(tipo);
        var vm = new TabelaVM
        {
            Titulo = t.Titulo,
            Subtitulo = "Localidades parametrizadas",
            Colunas = { "Código", "Descrição", t.Extra },
            AcoesTopo = { AcaoVM.Link("Novo registo", $"/Configuracao/CadastroNovo?tipo={tipo}", "ti-plus") }
        };
        foreach (var item in M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Descricao))
        {
            var pai = paiTipo is null || !int.TryParse(item.Extra, out var paiId)
                ? "—"
                : M.Cadastros.FirstOrDefault(c => c.Id == paiId && c.Tipo == paiTipo)?.Descricao ?? "—";
            vm.Linhas.Add(LinhaVM.De(item.Codigo, item.Descricao, pai)
                .Com(AcaoVM.Link("Editar", $"/Configuracao/CadastroEditar/{item.Id}?tipo={tipo}", "ti-edit", "btn-ghost"),
                     AcaoVM.Envio("Eliminar", $"/Configuracao/CadastroEliminar/{item.Id}?tipo={tipo}", "Eliminar este registo?", "ti-trash", "btn-danger")));
        }
        return Tabela(vm);
    }

    private static string? TipoPai(string tipo) => tipo switch
    {
        "provincias" => "paises",
        "distritos" => "provincias",
        _ => null
    };

    private static FormVM FormLocalidade(string tipo, CadastroItem? item = null)
    {
        var t = Tipos[tipo];
        var paiTipo = TipoPai(tipo);
        var vm = new FormVM
        {
            Titulo = item is null ? $"{t.Titulo} — Novo registo" : $"{t.Titulo} — Editar registo",
            Subtitulo = "Parametrização de localidades",
            ActionUrl = item is null ? $"/Configuracao/CadastroNovo?tipo={tipo}" : $"/Configuracao/CadastroEditar/{item.Id}?tipo={tipo}",
            VoltarUrl = $"/Configuracao/Cadastro?tipo={tipo}"
        };
        vm.Campos.Add(CampoVM.Txt("Codigo", "Código", valor: item?.Codigo ?? ""));
        vm.Campos.Add(CampoVM.Txt("Descricao", "Descrição", valor: item?.Descricao ?? ""));
        if (paiTipo is not null)
            vm.Campos.Add(CampoVM.Sel("PaiId", t.Extra,
                M.Cadastros.Where(c => c.Tipo == paiTipo).OrderBy(c => c.Descricao).Select(c => (c.Id.ToString(), c.Descricao)).ToList(),
                item?.Extra ?? "", larg: 6));
        return vm;
    }

    private static bool ObterExtraLocalidade(string tipo, IFormCollection f, out string extra)
    {
        extra = "";
        var paiTipo = TipoPai(tipo);
        if (paiTipo is null) return true;
        if (!int.TryParse(f["PaiId"], out var paiId) || !M.Cadastros.Any(c => c.Id == paiId && c.Tipo == paiTipo)) return false;
        extra = paiId.ToString();
        return true;
    }

    private static FormVM FormCad(string tipo, (string Titulo, string Extra) t, CadastroItem? item = null) => new()
    {
        Titulo = item is null ? $"{t.Titulo} — Novo registo" : $"{t.Titulo} — Editar registo",
        Subtitulo = "Parametrização",
        ActionUrl = item is null ? $"/Configuracao/CadastroNovo?tipo={tipo}" : $"/Configuracao/CadastroEditar/{item.Id}?tipo={tipo}",
        VoltarUrl = $"/Configuracao/Cadastro?tipo={tipo}",
        Campos = { CampoVM.Txt("Codigo", "Código", valor: item?.Codigo ?? ""), CampoVM.Txt("Descricao", "Descrição", valor: item?.Descricao ?? ""), CampoVM.Txt("Extra", t.Extra, valor: item?.Extra ?? "", obrig: false, larg: 12) }
    };

    [HttpGet]
    public IActionResult CadastroNovo(string tipo)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        if (tipo == "tipos-seguro") return Form(FormTipoSeguro());
        return Form(tipo is "paises" or "provincias" or "distritos" ? FormLocalidade(tipo!) : FormCad(tipo!, t));
    }

    [HttpPost]
    public IActionResult CadastroNovo(string tipo, IFormCollection f)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        if (tipo == "tipos-seguro")
        {
            if (!TryTipoSeguro(f, out var novoTipoSeguro, out var erroTipoSeguro))
                return FormComErro(FormTipoSeguro(), f, erroTipoSeguro);
            var criado = _svc.GuardarCadastro(novoTipoSeguro);
            if (!criado.IsSuccess) return FormComErro(FormTipoSeguro(), f, criado.Error!);
            return Sucesso("Tipo de seguro criado.", "/Configuracao/Cadastro?tipo=tipos-seguro");
        }
        if (tipo is "paises" or "provincias" or "distritos")
        {
            if (!ObterExtraLocalidade(tipo!, f, out var extraLocalidade))
                return FormComErro(FormLocalidade(tipo!), f, $"Seleccione {t.Extra.ToLowerInvariant()}.");
            var localidade = _svc.GuardarCadastro(new CadastroItem
            {
                Tipo = tipo!, Codigo = f["Codigo"].ToString().Trim(), Descricao = f["Descricao"].ToString().Trim(), Extra = extraLocalidade
            });
            if (!localidade.IsSuccess) return FormComErro(FormLocalidade(tipo!), f, localidade.Error!);
            return Sucesso("Registo criado.", $"/Configuracao/Cadastro?tipo={tipo}");
        }
        var r = _svc.GuardarCadastro(new CadastroItem { Tipo = tipo!, Codigo = f["Codigo"].ToString().Trim(), Descricao = f["Descricao"].ToString().Trim(), Extra = f["Extra"].ToString().Trim() });
        if (!r.IsSuccess) return FormComErro(FormCad(tipo!, t), f, r.Error!);
        return Sucesso("Registo criado.", $"/Configuracao/Cadastro?tipo={tipo}");
    }

    [HttpGet]
    public IActionResult CadastroEditar(int id, string tipo)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        var item = M.Cadastros.FirstOrDefault(c => c.Id == id && c.Tipo == tipo);
        if (item is null) return Falha("Registo não encontrado.", $"/Configuracao/Cadastro?tipo={tipo}");
        if (tipo == "tipos-seguro") return Form(FormTipoSeguro(item));
        return Form(tipo is "paises" or "provincias" or "distritos"
            ? FormLocalidade(tipo!, item)
            : FormCad(tipo!, t, item));
    }

    [HttpPost]
    public IActionResult CadastroEditar(int id, string tipo, IFormCollection f)
    {
        if (!Tipos.TryGetValue(tipo ?? "", out var t)) return Falha("Cadastro desconhecido.", "/Dashboard/Index");
        var item = M.Cadastros.FirstOrDefault(c => c.Id == id && c.Tipo == tipo);
        if (item is null) return Falha("Registo não encontrado.", $"/Configuracao/Cadastro?tipo={tipo}");
        if (tipo == "tipos-seguro")
        {
            if (!TryTipoSeguro(f, out var dadosTipoSeguro, out var erroTipoSeguro))
                return FormComErro(FormTipoSeguro(item), f, erroTipoSeguro);
            var actualizadoSeguro = _svc.ActualizarCadastro(id, dadosTipoSeguro.Codigo, dadosTipoSeguro.Descricao, dadosTipoSeguro.Extra);
            if (!actualizadoSeguro.IsSuccess) return FormComErro(FormTipoSeguro(item), f, actualizadoSeguro.Error!);
            item.SeguradoraId = dadosTipoSeguro.SeguradoraId;
            item.Coeficiente = dadosTipoSeguro.Coeficiente;
            item.EncargoPct = dadosTipoSeguro.EncargoPct;
            item.SeloPct = dadosTipoSeguro.SeloPct;
            item.SobretaxaPct = dadosTipoSeguro.SobretaxaPct;
            item.ComissaoPct = dadosTipoSeguro.ComissaoPct;
            item.Estado = dadosTipoSeguro.Estado;
            return Sucesso("Tipo de seguro actualizado.", "/Configuracao/Cadastro?tipo=tipos-seguro");
        }
        string extra;
        var localidades = tipo is "paises" or "provincias" or "distritos";
        if (localidades)
        {
            if (!ObterExtraLocalidade(tipo!, f, out extra))
                return FormComErro(FormLocalidade(tipo!, item), f, $"Seleccione {t.Extra.ToLowerInvariant()}.");
        }
        else extra = f["Extra"].ToString().Trim();

        var actualizado = _svc.ActualizarCadastro(id, f["Codigo"].ToString().Trim(), f["Descricao"].ToString().Trim(), extra);
        if (!actualizado.IsSuccess)
            return FormComErro(localidades ? FormLocalidade(tipo!, item) : FormCad(tipo!, t, item), f, actualizado.Error!);
        return Sucesso("Registo actualizado.", $"/Configuracao/Cadastro?tipo={tipo}");
    }

    [HttpPost]
    public IActionResult CadastroEliminar(int id, string tipo)
    {
        var tipoSeguro = tipo == "tipos-seguro" ? M.Cadastros.FirstOrDefault(c => c.Id == id && c.Tipo == tipo) : null;
        if (tipoSeguro is not null && M.Apolices.Any(a => a.TipoSeguroId == id || a.TipoSeguro == tipoSeguro.Descricao))
            return Falha("Não é possível eliminar este tipo de seguro porque já está associado a apólices.", $"/Configuracao/Cadastro?tipo={tipo}");

        var usadoPorCliente = tipo switch
        {
            "paises" => M.Clientes.Any(c => c.PaisId == id),
            "provincias" => M.Clientes.Any(c => c.ProvinciaId == id),
            "distritos" => M.Clientes.Any(c => c.DistritoId == id),
            _ => false
        };
        if (usadoPorCliente)
            return Falha("Não é possível eliminar esta localidade porque está associada a clientes.", $"/Configuracao/Cadastro?tipo={tipo}");

        var filhoTipo = tipo == "paises" ? "provincias" : tipo == "provincias" ? "distritos" : null;
        if (filhoTipo is not null && M.Cadastros.Any(c => c.Tipo == filhoTipo && c.Extra == id.ToString()))
            return Falha("Não é possível eliminar este registo enquanto tiver localidades associadas.", $"/Configuracao/Cadastro?tipo={tipo}");
        var r = _svc.EliminarCadastro(id);
        return r.IsSuccess ? Sucesso("Registo eliminado.", $"/Configuracao/Cadastro?tipo={tipo}") : Falha(r.Error!, $"/Configuracao/Cadastro?tipo={tipo}");
    }

    // ======================= Seguradoras =======================
    public IActionResult Seguradoras()
    {
        var vm = new TabelaVM
        {
            Titulo = "Seguradoras", Subtitulo = "Seguradoras parceiras",
            Colunas = { "Sigla", "Nome", "NUIT", "Telefone", "Prefixo da referência" },
            AcoesTopo = { AcaoVM.Link("Nova seguradora", "/Configuracao/SeguradoraNova", "ti-plus") }
        };
        foreach (var s in M.Seguradoras) vm.Linhas.Add(LinhaVM.De(s.Sigla, s.Nome, s.Nuit, s.Telefone, s.ReferenciaPadrao)
            .Com(AcaoVM.Link("Editar", $"/Configuracao/SeguradoraEditar/{s.Id}", "ti-edit", "btn-ghost")));
        return Tabela(vm);
    }

    private static FormVM FormSeg(Seguradora? seguradora = null) => new()
    {
        Titulo = seguradora is null ? "Nova Seguradora" : "Editar Seguradora",
        Subtitulo = "Parametrização",
        ActionUrl = seguradora is null ? "/Configuracao/SeguradoraNova" : $"/Configuracao/SeguradoraEditar/{seguradora.Id}",
        VoltarUrl = "/Configuracao/Seguradoras",
        Campos =
        {
            CampoVM.Txt("Nome", "Nome", valor: seguradora?.Nome ?? ""),
            CampoVM.Txt("Sigla", "Sigla", valor: seguradora?.Sigla ?? ""),
            CampoVM.Txt("Nuit", "NUIT", obrig: false, valor: seguradora?.Nuit ?? ""),
            CampoVM.Txt("Telefone", "Telefone", obrig: false, valor: seguradora?.Telefone ?? ""),
            CampoVM.Txt("ReferenciaPadrao", "Prefixo da referência", obrig: false, valor: seguradora?.ReferenciaPadrao ?? "")
        }
    };

    [HttpGet] public IActionResult SeguradoraNova() => Form(FormSeg());

    [HttpPost]
    public IActionResult SeguradoraNova(IFormCollection f)
    {
        var r = _svc.GuardarSeguradora(new Seguradora { Nome = f["Nome"].ToString(), Sigla = f["Sigla"].ToString().ToUpper(), Nuit = f["Nuit"].ToString(), Telefone = f["Telefone"].ToString(), ReferenciaPadrao = f["ReferenciaPadrao"].ToString().Trim() });
        if (!r.IsSuccess) return FormComErro(FormSeg(), f, r.Error!);
        return Sucesso("Seguradora criada.", "/Configuracao/Seguradoras");
    }

    [HttpGet]
    public IActionResult SeguradoraEditar(int id)
    {
        var seguradora = M.Seg(id);
        return seguradora is null ? Falha("Seguradora não encontrada.", "/Configuracao/Seguradoras") : Form(FormSeg(seguradora));
    }

    [HttpPost]
    public IActionResult SeguradoraEditar(int id, IFormCollection f)
    {
        var seguradora = M.Seg(id);
        if (seguradora is null) return Falha("Seguradora não encontrada.", "/Configuracao/Seguradoras");
        seguradora.Nome = f["Nome"].ToString().Trim();
        seguradora.Sigla = f["Sigla"].ToString().Trim().ToUpperInvariant();
        seguradora.Nuit = f["Nuit"].ToString().Trim();
        seguradora.Telefone = f["Telefone"].ToString().Trim();
        seguradora.ReferenciaPadrao = f["ReferenciaPadrao"].ToString().Trim();
        if (string.IsNullOrWhiteSpace(seguradora.Nome) || string.IsNullOrWhiteSpace(seguradora.Sigla))
        {
            var vm = FormSeg(seguradora);
            Preencher(vm, f);
            ModelState.AddModelError(string.Empty, "Informe o nome e a sigla da seguradora.");
            return Form(vm);
        }
        return Sucesso("Seguradora actualizada.", "/Configuracao/Seguradoras");
    }

    // ======================= Seguradoras e Clientes =======================
    public IActionResult SeguradorasClientes()
    {
        var vm = new TabelaVM
        {
            Titulo = "Seguradoras e Clientes", Subtitulo = "Carteira e comissões por tipo de seguro",
            Colunas = { "Seguradora", "Tipos de seguro / comissão", "Apólices activas", "Clientes", "Prémios activos" }
        };
        foreach (var s in M.Seguradoras)
        {
            var ap = M.Apolices.Where(a => a.SeguradoraId == s.Id && a.Estado == "Activa").ToList();
            var tipos = M.Cadastros.Where(c => c.Tipo == "tipos-seguro" && c.SeguradoraId == s.Id)
                .OrderBy(c => c.Descricao).Select(c => $"{c.Descricao}: {Fmt.Num(c.ComissaoPct)}%");
            vm.Linhas.Add(LinhaVM.De(s.Sigla, string.Join(", ", tipos.DefaultIfEmpty("—")), ap.Count.ToString(), ap.Select(a => a.ClienteId).Distinct().Count().ToString(), Fmt.Mt(ap.Sum(a => a.Premio))));
        }
        return Tabela(vm);
    }
}
