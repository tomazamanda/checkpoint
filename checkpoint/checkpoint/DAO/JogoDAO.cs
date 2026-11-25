using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class JogoDAO : IDao<Jogo>
    {
        public void Insert(Jogo jogo)
        {
            string sql = "INSERT INTO jogo (nome, genero, classificacaoIndicativa) VALUES (@nome, @genero, @classificacao)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", jogo.Nome);
                cmd.Parameters.AddWithValue("@genero", Converte.ParaBanco(jogo.Genero));
                cmd.Parameters.AddWithValue("@classificacao", jogo.ClassificacaoIndicativa);
                cmd.ExecuteNonQuery();
                jogo.JogoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Jogo jogo)
        {
            string sql = @"UPDATE jogo SET nome = @nome, genero = @genero, classificacaoIndicativa = @classificacao
                WHERE jogoId = @jogoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", jogo.Nome);
                cmd.Parameters.AddWithValue("@genero", Converte.ParaBanco(jogo.Genero));
                cmd.Parameters.AddWithValue("@classificacao", jogo.ClassificacaoIndicativa);
                cmd.Parameters.AddWithValue("@jogoId", jogo.JogoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum jogo foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM jogo WHERE jogoId = @jogoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@jogoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum jogo encontrado com esse ID.");
            }
        }

        public List<Jogo> List()
        {
            List<Jogo> jogos = new List<Jogo>();
            string sql = "SELECT * FROM jogo ORDER BY nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    jogos.Add(Mapear(dr));
            }
            return jogos;
        }

        public Jogo BuscarID(int id)
        {
            string sql = "SELECT * FROM jogo WHERE jogoId = @jogoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@jogoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum jogo encontrado com esse ID.");
                }
            }
        }

        private Jogo Mapear(MySqlDataReader dr)
        {
            Jogo jogo = new Jogo();
            jogo.JogoId = Convert.ToInt32(dr["jogoId"]);
            jogo.Nome = dr["nome"].ToString();
            jogo.Genero = Converte.Texto(dr, "genero");
            jogo.ClassificacaoIndicativa = Convert.ToInt32(dr["classificacaoIndicativa"]);
            return jogo;
        }
    }
}
