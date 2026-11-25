using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ResponsavelDAO : IDao<Responsavel>
    {
        private const string SelectBase = @"SELECT r.*, p.*
            FROM responsavel r
            INNER JOIN pessoa p ON p.pessoaId = r.FK_PessoaId";

        public void Insert(Responsavel responsavel)
        {
            using (var conexao = Conexao.Conectar())
            // Pessoa e responsável são gravados juntos numa transação (ver ItemComandaDAO.Insert).
            using (var transacao = conexao.BeginTransaction())
            {
                if (responsavel.PessoaId == 0)
                    PessoaDAO.Insert(responsavel, conexao, transacao);

                string sql = "INSERT INTO responsavel (FK_PessoaId) VALUES (@pessoaId)";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@pessoaId", responsavel.PessoaId);
                    cmd.ExecuteNonQuery();
                    responsavel.ResponsavelId = (int)cmd.LastInsertedId;
                }
                transacao.Commit();
            }
        }

        // responsavel não tem colunas próprias: atualizar = atualizar a pessoa
        public void Update(Responsavel responsavel)
        {
            using (var conexao = Conexao.Conectar())
            {
                PessoaDAO.Update(responsavel, conexao, null);
            }
        }

        public void Delete(int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                int pessoaId = PessoaDAO.PessoaIdDe("responsavel", "responsavelId", id, conexao, transacao);

                string sql = "DELETE FROM responsavel WHERE responsavelId = @responsavelId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@responsavelId", id);
                    cmd.ExecuteNonQuery();
                }
                PessoaDAO.DeleteSeNaoUsada(pessoaId, conexao, transacao);
                transacao.Commit();
            }
        }

        public List<Responsavel> List()
        {
            List<Responsavel> responsaveis = new List<Responsavel>();
            string sql = SelectBase + " ORDER BY p.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    responsaveis.Add(Mapear(dr));
            }
            return responsaveis;
        }

        public Responsavel BuscarID(int id)
        {
            string sql = SelectBase + " WHERE r.responsavelId = @responsavelId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@responsavelId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum responsável encontrado com esse ID.");
                }
            }
        }

        private Responsavel Mapear(MySqlDataReader dr)
        {
            Responsavel responsavel = new Responsavel();
            PessoaDAO.Mapear(dr, responsavel);
            responsavel.ResponsavelId = Convert.ToInt32(dr["responsavelId"]);
            return responsavel;
        }
    }
}
