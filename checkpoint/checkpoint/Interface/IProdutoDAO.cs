using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IProdutoDAO : IDao<Produto>
    {
        List<Produto> EstoqueBaixo();
        void ReporEstoque(int produtoId, int quantidade);
    }
}