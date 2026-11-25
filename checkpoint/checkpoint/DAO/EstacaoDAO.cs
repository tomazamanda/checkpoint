using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class EstacaoDAO : IEstacaoDAO
    {
        public void Insert(Estacao estacao)
        {
            string sql = @"INSERT INTO estacao (numero, tipo, plataforma, status, precoHora)
                VALUES (@numero, @tipo, @plataforma, @status, @precoHora)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, estacao);
                cmd.ExecuteNonQuery();
                estacao.EstacaoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Estacao estacao)
        {
            string sql = @"UPDATE estacao SET numero = @numero, tipo = @tipo, plataforma = @plataforma,
                status = @status, precoHora = @precoHora WHERE estacaoId = @estacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, estacao);
                cmd.Parameters.AddWithValue("@estacaoId", estacao.EstacaoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma estação foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM estacao WHERE estacaoId = @estacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@estacaoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma estação encontrada com esse ID.");
            }
        }

        public List<Estacao> List()
        {
            return Listar("SELECT * FROM estacao ORDER BY numero");
        }

        public List<Estacao> ListarDisponiveis()
        {
            return Listar("SELECT * FROM estacao WHERE status = 'disponivel' ORDER BY numero");
        }

        // COALESCE: estação sem jogo instalado dá MAX de nada, que é NULL; vira 0 (livre para todas as idades)
        public int ClassificacaoMaxima(int estacaoId)
        {
            string sql = @"SELECT COALESCE(MAX(j.classificacaoIndicativa), 0)
                FROM jogoEstacao je
                INNER JOIN jogo j ON j.jogoId = je.FK_JogoId
                WHERE je.FK_EstacaoId = @estacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@estacaoId", estacaoId);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    return Convert.ToInt32(dr[0]);   // a consulta devolve um número só
                }
            }
        }

        public Estacao BuscarID(int id)
        {
            string sql = "SELECT * FROM estacao WHERE estacaoId = @estacaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@estacaoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma estação encontrada com esse ID.");
                }
            }
        }

        private List<Estacao> Listar(string sql)
        {
            List<Estacao> estacoes = new List<Estacao>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    estacoes.Add(Mapear(dr));
            }
            return estacoes;
        }

        private void Parametros(MySqlCommand cmd, Estacao estacao)
        {
            cmd.Parameters.AddWithValue("@numero", estacao.Numero);
            cmd.Parameters.AddWithValue("@tipo", estacao.Tipo);
            cmd.Parameters.AddWithValue("@plataforma", estacao.Plataforma);
            cmd.Parameters.AddWithValue("@status", estacao.Status);
            cmd.Parameters.AddWithValue("@precoHora", estacao.PrecoHora);
        }

        private Estacao Mapear(MySqlDataReader dr)
        {
            Estacao estacao = new Estacao();
            estacao.EstacaoId = Convert.ToInt32(dr["estacaoId"]);
            estacao.Numero = Convert.ToInt32(dr["numero"]);
            estacao.Tipo = dr["tipo"].ToString();
            estacao.Plataforma = dr["plataforma"].ToString();
            estacao.Status = dr["status"].ToString();
            estacao.PrecoHora = Convert.ToDouble(dr["precoHora"]);
            return estacao;
        }
    }
}
