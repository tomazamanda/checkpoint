namespace checkpoint.Controllers
{
    // O que o front envia em cada operação, são os mesmos dados que o menu de console pedia pelo teclado: quase sempre IDs, que o controller transforma em objetos com o BuscarID. 

    public class NovoCliente
    {
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public DateOnly? DataNasc { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public int? ResponsavelId { get; set; }
        public string Parentesco { get; set; }
    }

    public class NovaSessao
    {
        public int ClienteId { get; set; }
        public int EstacaoId { get; set; }
        public string Origem { get; set; }
        public int? ComandaId { get; set; }
    }

    public class Login
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }

    // O funcionário do caixa e o avaliador não vêm do front: são quem está logado.
    public class AberturaCaixa
    {
        public double ValorAbertura { get; set; }
    }

    public class NovaComanda
    {
        public int? ClienteId { get; set; }
    }

    public class NovoItem
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class NovoProduto
    {
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public string Unidade { get; set; }
        public double? PrecoVenda { get; set; }
        public int QtdEstoque { get; set; }
        public int QtdMinima { get; set; }
    }

    public class Reposicao
    {
        public int Quantidade { get; set; }
    }

    public class FechamentoComanda
    {
        public string FormaPagamento { get; set; }
    }

    public class NovoResumo
    {
        public int ClienteId { get; set; }
        public int ObraId { get; set; }
    }

    public class NovaResolucao
    {
        public int ClienteId { get; set; }
        public int DesafioId { get; set; }
    }

    public class Avaliacao
    {
        public bool Aprovado { get; set; }
        public string Observacao { get; set; }
    }
}
