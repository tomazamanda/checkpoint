using checkpoint.Mapeamento;

namespace checkpoint.Interface
{
    public interface IFuncionarioDAO : IDao<Funcionario>
    {
        Funcionario BuscarPorEmail(string email);   // null quando o e-mail não é de funcionário
    }
}
