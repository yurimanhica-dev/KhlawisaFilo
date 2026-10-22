namespace Klahisa.Web.Features.Clientes;

public class ClientesController(CorretoraService _svc) : BaseController
{

    private static CampoVM Campo(CampoVM campo, string secao, string? tipo = null, bool? obrigatorio = null)
    {
        campo.Secao = secao;
        campo.Aba = secao switch
        {
            "Dados profissionais" or "Dados da empresa" => "Profissional / Empresa",
            "Observações" => "Observações",
            _ => secao
        };
        campo.MostrarParaTipo = tipo;
        if (obrigatorio.HasValue) campo.Obrigatorio = obrigatorio.Value;
        return campo;
    }

    private static CampoVM CampoLocalidade(string nome, string rotulo, string tipo, string secao, int largura, string? dependeDe = null)
    {
        var registos = M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Descricao).ToList();
        var campo = CampoVM.Sel(nome, rotulo, registos.Select(c => (c.Id.ToString(), c.Descricao)).ToList(), larg: largura);
        campo.DependeDe = dependeDe ?? "";
        if (dependeDe is not null) campo.OpcaoPai = registos.ToDictionary(c => c.Id.ToString(), c => c.Extra);
        return Campo(campo, secao);
    }

    private static CampoVM CampoCadastro(string nome, string rotulo, string tipo, string secao, int largura)
    {
        var opcoes = M.Cadastros.Where(c => c.Tipo == tipo).OrderBy(c => c.Descricao)
            .Select(c => (c.Id.ToString(), c.Descricao)).ToList();
        return Campo(CampoVM.Sel(nome, rotulo, opcoes, obrig: false, larg: largura), secao);
    }

    private static string? DescricaoCadastro(string tipo, string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return "";
        return int.TryParse(id, out var codigo)
            ? M.Cadastros.FirstOrDefault(c => c.Id == codigo && c.Tipo == tipo)?.Descricao
            : null;
    }

    private static FormVM FormCliente() => new()
    {
        Titulo = "Cadastro de Cliente",
        Subtitulo = "Identificação, documentação e contactos",
        ActionUrl = "/Clientes/Novo",
        VoltarUrl = "/Clientes/Lista",
        UsaAbas = true,
        Info = "Os dados profissionais e empresariais aparecem conforme o tipo de cliente. Recolha dados de diligência apenas quando forem necessários ao produto ou processo.",
        Campos =
        {
            Campo(CampoVM.Sel("Tipo", "Tipo de cliente", Opcoes.De("Particular", "Empresa"), larg: 3), "Identificação"),
            Campo(CampoVM.Txt("Nome", "Nome completo / denominação social", larg: 6), "Identificação"),
            Campo(CampoVM.Txt("Nuit", "NUIT", larg: 3), "Identificação"),
            Campo(CampoVM.Dat("DataNascimento", "Data de nascimento", obrig: false, larg: 3), "Identificação", "Particular", true),
            Campo(CampoVM.Sel("Sexo", "Sexo", Opcoes.De("Feminino", "Masculino", "Outro"), obrig: false, larg: 3), "Identificação", "Particular"),
            Campo(CampoVM.Sel("EstadoCivil", "Estado civil", Opcoes.De("Solteiro(a)", "Casado(a)", "Divorciado(a)", "Viúvo(a)", "União de facto"), obrig: false, larg: 3), "Identificação", "Particular"),
            Campo(CampoCadastro("NacionalidadeId", "Nacionalidade", "nacionalidades", "Identificação", 3), "Identificação", "Particular"),
            Campo(CampoVM.Txt("NomePai", "Nome do pai", obrig: false, larg: 6), "Identificação", "Particular"),
            Campo(CampoVM.Txt("NomeMae", "Nome da mãe", obrig: false, larg: 6), "Identificação", "Particular"),

            Campo(CampoVM.Sel("TipoDocumento", "Tipo de documento", Opcoes.De("BI", "Passaporte", "DIRE", "Outro"), larg: 3), "Documentação", "Particular", true),
            Campo(CampoVM.Txt("NumDocumento", "Número do documento", larg: 3), "Documentação", "Particular", true),
            Campo(CampoVM.Dat("DataEmissaoDocumento", "Data de emissão", obrig: false, larg: 3), "Documentação", "Particular"),
            Campo(CampoVM.Dat("DataValidadeDocumento", "Data de validade", obrig: false, larg: 3), "Documentação", "Particular"),
            Campo(CampoVM.Txt("EntidadeEmissora", "Entidade emissora", obrig: false, larg: 6), "Documentação", "Particular"),

            Campo(CampoVM.Txt("Telefone", "Telefone principal", larg: 3), "Contactos e endereço"),
            Campo(CampoVM.Txt("TelefoneAlternativo", "Telefone alternativo", obrig: false, larg: 3), "Contactos e endereço"),
            Campo(CampoVM.Txt("Email", "E-mail", obrig: false, larg: 6, tipo: "email"), "Contactos e endereço"),
            CampoLocalidade("PaisId", "País", "paises", "Contactos e endereço", 3),
            CampoLocalidade("ProvinciaId", "Província", "provincias", "Contactos e endereço", 3, "PaisId"),
            CampoLocalidade("DistritoId", "Distrito", "distritos", "Contactos e endereço", 3, "ProvinciaId"),
            Campo(CampoVM.Txt("Cidade", "Cidade / município", obrig: false, larg: 3), "Contactos e endereço"),
            Campo(CampoVM.Txt("Bairro", "Bairro", obrig: false, larg: 3), "Contactos e endereço"),
            Campo(CampoVM.Txt("Endereco", "Endereço físico", obrig: false, larg: 6), "Contactos e endereço"),
            Campo(CampoVM.Txt("EnderecoPostal", "Endereço postal", obrig: false, larg: 3), "Contactos e endereço"),

            Campo(CampoCadastro("ProfissaoId", "Profissão", "profissoes", "Dados profissionais", 6), "Dados profissionais", "Particular"),
            Campo(CampoCadastro("EntidadeEmpregadoraId", "Entidade empregadora", "entidades-empregadoras", "Dados profissionais", 6), "Dados profissionais", "Particular"),
            Campo(CampoVM.Num("RendimentoMensal", "Rendimento mensal (MT)", obrig: false, larg: 3), "Dados profissionais", "Particular"),

            Campo(CampoVM.Txt("RegistoComercial", "Número de registo comercial", larg: 6), "Dados da empresa", "Empresa", true),
            Campo(CampoVM.Txt("RepresentanteLegal", "Representante legal", obrig: false, larg: 6), "Dados da empresa", "Empresa"),
            Campo(CampoVM.Txt("NuitRepresentante", "NUIT do representante", obrig: false, larg: 3), "Dados da empresa", "Empresa"),
            Campo(CampoVM.Sel("TipoDocumentoRepresentante", "Tipo de documento do representante", Opcoes.De("BI", "Passaporte", "DIRE", "Outro"), obrig: false, larg: 3), "Dados da empresa", "Empresa"),
            Campo(CampoVM.Txt("DocumentoRepresentante", "Documento do representante", obrig: false, larg: 3), "Dados da empresa", "Empresa"),
            Campo(CampoVM.Txt("PessoaContacto", "Pessoa responsável pelo seguro", larg: 6), "Dados da empresa", "Empresa", true),
            Campo(CampoVM.Txt("TelefoneContacto", "Telefone da pessoa responsável", larg: 3), "Dados da empresa", "Empresa", true),

            Campo(CampoVM.Area("Observacoes", "Observações", obrig: false), "Observações")
        }
    };

    [HttpGet]
    public IActionResult Novo() => Form(FormCliente());

    [HttpPost]
    public IActionResult Novo(IFormCollection f)
    {
        var tipo = f["Tipo"].ToString();
        if (tipo is not ("Particular" or "Empresa"))
            return FormComErro(FormCliente(), f, "Seleccione Particular ou Empresa.");

        if (string.IsNullOrWhiteSpace(f["Nome"]) || string.IsNullOrWhiteSpace(f["Nuit"]) || string.IsNullOrWhiteSpace(f["Telefone"]))
            return FormComErro(FormCliente(), f, "Informe o nome, NUIT e telefone principal do cliente.");

        var dataNascimento = DataOpcional(f["DataNascimento"]);
        if (tipo == "Particular" && (dataNascimento is null
            || string.IsNullOrWhiteSpace(f["TipoDocumento"]) || string.IsNullOrWhiteSpace(f["NumDocumento"])))
            return FormComErro(FormCliente(), f, "Para clientes particulares, informe a data de nascimento e os dados do documento de identificação.");

        if (tipo == "Empresa" && (string.IsNullOrWhiteSpace(f["RegistoComercial"])
            || string.IsNullOrWhiteSpace(f["PessoaContacto"]) || string.IsNullOrWhiteSpace(f["TelefoneContacto"])))
            return FormComErro(FormCliente(), f, "Para empresas, informe o registo comercial e os dados da pessoa responsável pelo seguro.");

        if (tipo == "Empresa" && (string.IsNullOrWhiteSpace(f["TipoDocumentoRepresentante"]) != string.IsNullOrWhiteSpace(f["DocumentoRepresentante"])))
            return FormComErro(FormCliente(), f, "Informe o tipo e o número do documento do representante, ou deixe ambos vazios.");

        decimal? rendimentoMensal = null;
        if (tipo == "Particular" && !string.IsNullOrWhiteSpace(f["RendimentoMensal"]))
        {
            rendimentoMensal = Dec(f["RendimentoMensal"]);
            if (rendimentoMensal < 0)
                return FormComErro(FormCliente(), f, "O rendimento mensal não pode ser negativo.");
        }

        var pais = M.Cadastros.FirstOrDefault(c => c.Id == Int(f["PaisId"]) && c.Tipo == "paises");
        var provincia = M.Cadastros.FirstOrDefault(c => c.Id == Int(f["ProvinciaId"]) && c.Tipo == "provincias");
        var distrito = M.Cadastros.FirstOrDefault(c => c.Id == Int(f["DistritoId"]) && c.Tipo == "distritos");
        if (pais is null || provincia is null || provincia.Extra != pais.Id.ToString()
            || distrito is null || distrito.Extra != provincia.Id.ToString())
            return FormComErro(FormCliente(), f, "Seleccione um país, uma província desse país e um distrito dessa província.");

        var nacionalidade = DescricaoCadastro("nacionalidades", f["NacionalidadeId"]);
        var profissao = DescricaoCadastro("profissoes", f["ProfissaoId"]);
        var entidadeEmpregadora = DescricaoCadastro("entidades-empregadoras", f["EntidadeEmpregadoraId"]);
        if (nacionalidade is null || profissao is null || entidadeEmpregadora is null)
            return FormComErro(FormCliente(), f, "Seleccione valores válidos para nacionalidade, profissão e entidade empregadora.");

        var c = new Cliente
        {
            Tipo = tipo,
            Nome = f["Nome"].ToString().Trim(),
            Nuit = f["Nuit"].ToString().Trim(),
            TipoDocumento = tipo == "Particular" ? f["TipoDocumento"].ToString() : "",
            NumDocumento = tipo == "Particular" ? f["NumDocumento"].ToString() : "",
            DataNascimento = tipo == "Particular" ? dataNascimento : null,
            Sexo = tipo == "Particular" ? f["Sexo"].ToString() : "",
            EstadoCivil = tipo == "Particular" ? f["EstadoCivil"].ToString() : "",
            Nacionalidade = tipo == "Particular" ? nacionalidade : "",
            NomePai = tipo == "Particular" ? f["NomePai"].ToString() : "",
            NomeMae = tipo == "Particular" ? f["NomeMae"].ToString() : "",
            DataEmissaoDocumento = tipo == "Particular" ? DataOpcional(f["DataEmissaoDocumento"]) : null,
            DataValidadeDocumento = tipo == "Particular" ? DataOpcional(f["DataValidadeDocumento"]) : null,
            EntidadeEmissora = tipo == "Particular" ? f["EntidadeEmissora"].ToString() : "",
            Telefone = f["Telefone"].ToString(),
            TelefoneAlternativo = f["TelefoneAlternativo"].ToString(),
            Email = f["Email"].ToString(),
            Endereco = f["Endereco"].ToString(),
            Cidade = f["Cidade"].ToString(),
            Distrito = distrito.Descricao,
            Provincia = provincia.Descricao,
            Bairro = f["Bairro"].ToString(),
            Pais = pais.Descricao,
            PaisId = pais.Id,
            ProvinciaId = provincia.Id,
            DistritoId = distrito.Id,
            EnderecoPostal = f["EnderecoPostal"].ToString(),
            Profissao = tipo == "Particular" ? profissao : "",
            EntidadeEmpregadora = tipo == "Particular" ? entidadeEmpregadora : "",
            RendimentoMensal = rendimentoMensal,
            RegistoComercial = tipo == "Empresa" ? f["RegistoComercial"].ToString() : "",
            RepresentanteLegal = tipo == "Empresa" ? f["RepresentanteLegal"].ToString() : "",
            NuitRepresentante = tipo == "Empresa" ? f["NuitRepresentante"].ToString() : "",
            TipoDocumentoRepresentante = tipo == "Empresa" ? f["TipoDocumentoRepresentante"].ToString() : "",
            DocumentoRepresentante = tipo == "Empresa" ? f["DocumentoRepresentante"].ToString() : "",
            PessoaContacto = tipo == "Empresa" ? f["PessoaContacto"].ToString() : "",
            TelefoneContacto = tipo == "Empresa" ? f["TelefoneContacto"].ToString() : "",
            Observacoes = f["Observacoes"].ToString()
        };
        var r = _svc.RegistarCliente(c);
        if (!r.IsSuccess) return FormComErro(FormCliente(), f, r.Error!);
        return Sucesso($"Cliente {r.Value!.Codigo} registado com sucesso.", "/Clientes/Lista");
    }

    private static DateTime? DataOpcional(string? valor) =>
        DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data) ? data : null;

    public IActionResult Lista()
    {
        var vm = new TabelaVM
        {
            Titulo = "Listagem de Clientes",
            Subtitulo = "Todos os clientes registados",
            Colunas = { "Código", "Nome", "Tipo", "NUIT", "Telefone", "Email", "Apólices", "Registo" },
            AcoesTopo = { AcaoVM.Link("Novo cliente", "/Clientes/Novo", "ti-plus") }
        };
        foreach (var c in M.Clientes.OrderBy(c => c.Id))
            vm.Linhas.Add(LinhaVM.De(c.Codigo, c.Nome, c.Tipo, c.Nuit, c.Telefone, c.Email,
                M.Apolices.Count(a => a.ClienteId == c.Id).ToString(), Fmt.Dt(c.DataRegisto)));
        return Tabela(vm);
    }
}
