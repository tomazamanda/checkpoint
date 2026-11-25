using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface ISessaoDAO : IDao<Sessao>
    {
        List<Sessao> ListarAbertas();
        List<Sessao> ListarPorComanda(int comandaId);
    }
}