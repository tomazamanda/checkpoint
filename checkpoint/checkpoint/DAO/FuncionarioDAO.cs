using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class FuncionarioDAO : IFuncionarioDAO
    {
        private const string SelectBase = @"SELECT f.*, p.*
            FROM funcionario f
            INNER JOIN pessoa p ON p.pessoaId = f.FK_PessoaId";

        public void Insert(Funcionario funcionario)
        {
            using (var conexao = Conexao.Conectar())
            // Pessoa e funcionário são gravados juntos numa transação (ver ItemComandaDAO.Insert).
            using (var transacao = conexao.BeginTransaction())
            {
                if (funcionario.PessoaId == 0)
                    PessoaDAO.Insert(funcionario, conexao, transacao);

                string sql = @"INSERT INTO funcionario (cargo, dataAdmissao, FK_PessoaId)
                    VALUES (@cargo, @dataAdmissao, @pessoaId)";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@cargo", funcionario.Cargo);
                    cmd.Parameters.AddWithValue("@dataAdmissao", Converte.ParaBanco(funcionario.DataAdmissao));
                    cmd.Parameters.AddWithValue("@pessoaId", funcionario.PessoaId);
                    cmd.ExecuteNonQuery();
                    funcionario.FuncionarioId = (int)cmd.LastInsertedId;
                }
                transacao.Commit();
            }
        }

        public void Update(Funcionario funcionario)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                PessoaDAO.Update(funcionario, conexao, transacao);

                string sql = @"UPDATE funcionario SET cargo = @cargo, dataAdmissao = @dataAdmissao
                    WHERE funcionarioId = @funcionarioId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@cargo", funcionario.Cargo);
                    cmd.Parameters.AddWithValue("@dataAdmissao", Converte.ParaBanco(funcionario.DataAdmissao));
                    cmd.Parameters.AddWithValue("@funcionarioId", funcionario.FuncionarioId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Nenhum funcionário foi atualizado (verifique o ID).");
                }
                transacao.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                int pessoaId = PessoaDAO.PessoaIdDe("funcionario", "funcionarioId", id, conexao, transacao);

                string sql = "DELETE FROM funcionario WHERE funcionarioId = @funcionarioId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@funcionarioId", id);
                    cmd.ExecuteNonQuery();
                }
                PessoaDAO.DeleteSeNaoUsada(pessoaId, conexao, transacao);
                transacao.Commit();
            }
        }

        public List<Funcionario> List()
        {
            List<Funcionario> funcionarios = new List<Funcionario>();
            string sql = SelectBase + " ORDER BY p.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    funcionarios.Add(Mapear(dr));
            }
            return funcionarios;
        }

        public Funcionario BuscarID(int id)
        {
            string sql = SelectBase + " WHERE f.funcionarioId = @funcionarioId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionarioId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum funcionário encontrado com esse ID.");
                }
            }
        }

        // Usado no login. O e-mail é UNIQUE na tabela pessoa, então acha no máximo uma pessoa.
        public Funcionario BuscarPorEmail(string email)
        {
            string sql = SelectBase + " WHERE p.email = @email";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@email", email);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? Mapear(dr) : null;
                }
            }
        }

        private Funcionario Mapear(MySqlDataReader dr)
        {
            Funcionario funcionario = new Funcionario();
            PessoaDAO.Mapear(dr, funcionario);
            funcionario.FuncionarioId = Convert.ToInt32(dr["funcionarioId"]);
            funcionario.Cargo = dr["cargo"].ToString();
            funcionario.DataAdmissao = DateOnly.FromDateTime(Convert.ToDateTime(dr["dataAdmissao"]));
            funcionario.SenhaHash = Converte.Texto(dr, "senhaHash");
            return funcionario;
        }
    }
}
