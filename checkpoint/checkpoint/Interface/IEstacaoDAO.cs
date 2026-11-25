using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IEstacaoDAO : IDao<Estacao>
    {
        List<Estacao> ListarDisponiveis();
        int ClassificacaoMaxima(int estacaoId);   // maior classificação entre os jogos instalados; 0 sem jogos
    }
}