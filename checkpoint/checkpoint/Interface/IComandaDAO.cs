using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IComandaDAO : IDao<Comanda>
    {
        List<Comanda> ListarAbertas();
    }
}