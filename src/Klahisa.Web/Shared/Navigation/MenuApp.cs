namespace Klahisa.Web.Shared.Navigation;

public record MenuItem(string Texto, string Url)
{
    public bool Activo(string atual)
    {
        var path = Url.Split('?')[0];
        if (Url.Contains('?')) return string.Equals(Url, atual, StringComparison.OrdinalIgnoreCase);
        return string.Equals(path, atual.Split('?')[0], StringComparison.OrdinalIgnoreCase)
            || atual.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase);
    }
}

public class MenuGrupo
{
    public string Texto { get; set; } = "";
    public string Icone { get; set; } = "ti-circle-plus";
    public List<MenuItem> Items { get; set; } = new();
    public List<MenuGrupo> Sub { get; set; } = new();

    public bool Contem(string atual) => Items.Any(i => i.Activo(atual)) || Sub.Any(s => s.Contem(atual));
}

public static class MenuApp
{
    private static MenuItem Mi(string t, string u) => new(t, u);
    private static MenuGrupo G(string t, string icone, params MenuItem[] items) => new() { Texto = t, Icone = icone, Items = items.ToList() };
    private static MenuGrupo S(string t, params MenuItem[] items) => new() { Texto = t, Icone = "ti-chevron-right", Items = items.ToList() };
    private static MenuGrupo Com(this MenuGrupo g, params MenuGrupo[] subs) { g.Sub.AddRange(subs); return g; }

    public static readonly List<MenuGrupo> Grupos = new()
    {
        G("Dashboard", "ti-layout-dashboard", Mi("Visão geral", "/Dashboard/Index")),

        G("Gestão de Clientes", "ti-users", Mi("Cadastro de Cliente", "/Clientes/Novo"), Mi("Listagem de Clientes", "/Clientes/Lista")),

        G("Gestão de Seguros", "ti-shield-check",
            Mi("Pedido de Seguro", "/Seguros/Pedido"),
            Mi("Listagem de Seguros", "/Seguros/Lista"),
            Mi("Alocação de Débitos", "/Seguros/AlocacaoDebitos"),
            Mi("Prestação de Contas", "/Seguros/PrestacaoContas"),
            Mi("Inclusão e Exclusão", "/Seguros/InclusaoExclusao"),
            Mi("Nota de Débito e Crédito", "/Seguros/NotaDebitoCredito"))
          .Com(S("Relatórios de Seguros",
            Mi("Seguros por Renovar", "/Seguros/ARenovar"),
            Mi("Débitos por Vencer", "/Seguros/DebitosPorVencer"),
            Mi("Débitos Vencidos", "/Seguros/DebitosVencidos"),
            Mi("Débitos Pagos", "/Seguros/DebitosPagos"),
            Mi("Extracto do Cliente", "/Seguros/ExtractoCliente"),
            Mi("Lista de Pagamentos", "/Seguros/ListaPagamentos"),
            Mi("Saldo das Seguradoras", "/Seguros/SaldoSeguradoras"))),

        G("Caixa", "ti-cash",
            Mi("Lista de Débitos", "/Caixa/ListaDebitos"),
            Mi("Pagamentos do Dia", "/Caixa/PagamentosDoDia"),
            Mi("Pagamentos Parciais", "/Caixa/PagamentosParciais"),
            Mi("Anulação de Recibo", "/Caixa/AnularRecibo"),
            Mi("Anulação da Apólice", "/Caixa/AnularApolice"),
            Mi("Apólices Anuladas", "/Caixa/ApolicesAnuladas"),
            Mi("Lançar Débitos à Contabilidade", "/Caixa/LancarContabilidade")),

        G("Gestão de Sinistros", "ti-alert-octagon", Mi("Ocorrência", "/Sinistros/Ocorrencia"), Mi("Listagem de Sinistros", "/Sinistros/Lista")),

        G("Contabilidade", "ti-calculator", Mi("Conta Plano", "/Contabilidade/ContaPlano"))
          .Com(
            S("Operações",
                Mi("Lançamentos Contabilísticos", "/Contabilidade/Lancamentos"),
                Mi("Lançamento em Moeda Estrangeira", "/Contabilidade/LancamentoME"),
                Mi("Diário de Lançamentos", "/Contabilidade/Diario"),
                Mi("Extracto da Conta Razão", "/Contabilidade/ExtractoRazao"),
                Mi("Reimpressão de Lançamentos", "/Contabilidade/Reimpressao")),
            S("Relatórios",
                Mi("Balancete", "/Contabilidade/Balancete"),
                Mi("Balanço", "/Contabilidade/Balanco"),
                Mi("Demonstração de Resultados", "/Contabilidade/DemonstracaoResultados"),
                Mi("Apuramento de Resultados", "/Contabilidade/ApuramentoResultados"))),

        G("Configurações", "ti-settings")
          .Com(
            S("Pessoal",
                Mi("Meu Perfil", "/Conta/MeuPerfil"),
                Mi("Minhas Tarefas", "/Conta/MinhasTarefas"),
                Mi("Alterar Senha", "/Conta/AlterarSenha")),
            S("Gerais",
                Mi("Bancos", "/Configuracao/Cadastro?tipo=bancos"),
                Mi("Tipos de Cliente", "/Configuracao/Cadastro?tipo=tipos-cliente"),
                Mi("Tipos de Contacto", "/Configuracao/Cadastro?tipo=tipos-contacto"),
                Mi("Tipos de Documento de Identificação", "/Configuracao/Cadastro?tipo=tipos-documento"),
                Mi("Câmbios", "/Configuracao/Cadastro?tipo=cambios"),
                Mi("Contas Bancárias", "/Configuracao/Cadastro?tipo=contas-bancarias")),
            S("Seguros",
                Mi("Seguradoras", "/Configuracao/Seguradoras"),
                Mi("Ramos", "/Configuracao/Cadastro?tipo=ramos"),
                Mi("Tipos de Seguros", "/Configuracao/Cadastro?tipo=tipos-seguro"),
                Mi("Coberturas", "/Configuracao/Cadastro?tipo=coberturas"),
                Mi("Balcões das Seguradoras", "/Configuracao/Cadastro?tipo=balcoes")),
            S("Comissionamento",
                Mi("Seguradoras e Clientes", "/Configuracao/SeguradorasClientes"),
                Mi("Selos", "/Configuracao/Cadastro?tipo=selos"),
                Mi("Comissões", "/Configuracao/Cadastro?tipo=comissoes"))),

        G("Relatórios e Estatísticas", "ti-chart-bar",
            Mi("Relação de Seguradoras", "/Estatisticas/RelacaoSeguradoras"),
            Mi("Estatísticas Diárias", "/Estatisticas/EstatisticasDiarias"),
            Mi("Mapa ISSM", "/Estatisticas/MapaISSM")),
    };
}
