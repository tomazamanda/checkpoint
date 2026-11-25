using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IClienteDAO : IDao<Cliente>
    {
        int SaldoMinutosSociais(int clienteId);
        int MinutosGanhosNaSemana(int clienteId, DateTime dia);   // semana de segunda a domingo que contém o dia
    }
}
