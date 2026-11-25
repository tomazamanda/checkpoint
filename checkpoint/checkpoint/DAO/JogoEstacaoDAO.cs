using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class JogoEstacaoDAO : IDao<JogoEstacao>
    {
        private const string SelectBase = @"SELECT je.*, e.numero, j.nome AS nomeJogo, j.classificacaoIndicativa
            FROM jogoEstacao je
            INNER JOIN estacao e ON e.estacaoId = je.FK_EstacaoId
            INNER JOIN jogo j ON j.jogoId = je.FK_JogoId";

        public void Insert(JogoEstacao jogoEstacao)
        {
            string sql = @"INSERT INTO jogoEstacao (dataInstalacao, FK_EstacaoId, FK_JogoId)
                VALUES (@dataInstalacao, @estacaoId, @jogoId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@dataInstalacao", Converte.ParaBanco(jogoEstacao.DataInstalacao));
                cmd.Parameters.AddWithValue("@estacaoId", jogoEstacao.Estacao.EstacaoId);
                cmd.Parameters.AddWithValue("@jogoId", jogoEstacao.Jogo.JogoId);
                cmd.ExecuteNonQuery();
                jogoEstacao.JogoEstacaoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(JogoEstacao jogoEstacao)
        {
            string sql = @"UPDATE jogoEstacao SET dataInstalacao = @dataInstalacao, FK_EstacaoId = @estacaoId,
                FK_JogoId = @jogoId WHERE jogoEstacaoId = @jogoEstacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@dataInstalacao", Converte.ParaBanco(jogoEstacao.DataInstalacao));
                cmd.Parameters.AddWithValue("@estacaoId", jogoEstacao.Estacao.EstacaoId);
                cmd.Parameters.AddWithValue("@jogoId", jogoEstacao.Jogo.JogoId);
                cmd.Parameters.AddWithValue("@jogoEstacaoId", jogoEstacao.JogoEstacaoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma instalação foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM jogoEstacao WHERE jogoEstacaoId = @jogoEstacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@jogoEstacaoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma instalação encontrada com esse ID.");
            }
        }

        public List<JogoEstacao> List()
        {
            List<JogoEstacao> lista = new List<JogoEstacao>();
            string sql = SelectBase + " ORDER BY e.numero, j.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    lista.Add(Mapear(dr));
            }
            return lista;
        }

        public JogoEstacao BuscarID(int id)
        {
            string sql = SelectBase + " WHERE je.jogoEstacaoId = @jogoEstacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@jogoEstacaoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma instalação encontrada com esse ID.");
                }
            }
        }

        private JogoEstacao Mapear(MySqlDataReader dr)
        {
            JogoEstacao jogoEstacao = new JogoEstacao();
            jogoEstacao.JogoEstacaoId = Convert.ToInt32(dr["jogoEstacaoId"]);
            jogoEstacao.DataInstalacao = Converte.Data(dr, "dataInstalacao");
            jogoEstacao.Estacao = new Estacao();
            jogoEstacao.Estacao.EstacaoId = Convert.ToInt32(dr["FK_EstacaoId"]);
            jogoEstacao.Estacao.Numero = Convert.ToInt32(dr["numero"]);
            jogoEstacao.Jogo = new Jogo();
            jogoEstacao.Jogo.JogoId = Convert.ToInt32(dr["FK_JogoId"]);
            jogoEstacao.Jogo.Nome = dr["nomeJogo"].ToString();
            jogoEstacao.Jogo.ClassificacaoIndicativa = Convert.ToInt32(dr["classificacaoIndicativa"]);
            return jogoEstacao;
        }
    }
}
