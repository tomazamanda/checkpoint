using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    // Interface genérica usada por ResumoDAO e ResolucaoDAO (T é Resumo ou Resolucao)
    public interface IAtividadeSocialDAO<T> : IDao<T>
    {
        List<T> ListarEntregues();
    }
}