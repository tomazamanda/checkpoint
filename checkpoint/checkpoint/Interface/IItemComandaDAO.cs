using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IItemComandaDAO : IDao<ItemComanda>
    {
        List<ItemComanda> ListarPorComanda(int comandaId);
    }
}