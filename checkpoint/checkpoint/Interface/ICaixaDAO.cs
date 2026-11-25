using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface ICaixaDAO : IDao<Caixa>
    {
        Caixa BuscarAberto();
        double TotalComandasFechadas(int caixaId);
    }
}