using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class DesafioDAO : IDao<Desafio>
    {
        public void Insert(Desafio desafio)
        {
            string sql = @"INSERT INTO desafio (titulo, enunciado, area, nivel, gabarito, minutosRecompensa)
                VALUES (@titulo, @enunciado, @area, @nivel, @gabarito, @minutosRecompensa)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, desafio);
                cmd.ExecuteNonQuery();
                desafio.DesafioId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Desafio desafio)
        {
            string sql = @"UPDATE desafio SET titulo = @titulo, enunciado = @enunciado, area = @area, nivel = @nivel,
                gabarito = @gabarito, minutosRecompensa = @minutosRecompensa WHERE desafioId = @desafioId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, desafio);
                cmd.Parameters.AddWithValue("@desafioId", desafio.DesafioId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum desafio foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM desafio WHERE desafioId = @desafioId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@desafioId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum desafio encontrado com esse ID.");
            }
        }

        public List<Desafio> List()
        {
            List<Desafio> desafios = new List<Desafio>();
            string sql = "SELECT * FROM desafio ORDER BY area, nivel, titulo";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    desafios.Add(Mapear(dr));
            }
            return desafios;
        }

        public Desafio BuscarID(int id)
        {
            string sql = "SELECT * FROM desafio WHERE desafioId = @desafioId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@desafioId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum desafio encontrado com esse ID.");
                }
            }
        }

        private void Parametros(MySqlCommand cmd, Desafio desafio)
        {
            cmd.Parameters.AddWithValue("@titulo", desafio.Titulo);
            cmd.Parameters.AddWithValue("@enunciado", desafio.Enunciado);
            cmd.Parameters.AddWithValue("@area", desafio.Area);
            cmd.Parameters.AddWithValue("@nivel", desafio.Nivel);
            cmd.Parameters.AddWithValue("@gabarito", Converte.ParaBanco(desafio.Gabarito));
            cmd.Parameters.AddWithValue("@minutosRecompensa", desafio.MinutosRecompensa);
        }

        private Desafio Mapear(MySqlDataReader dr)
        {
            Desafio desafio = new Desafio();
            desafio.DesafioId = Convert.ToInt32(dr["desafioId"]);
            desafio.Titulo = dr["titulo"].ToString();
            desafio.Enunciado = dr["enunciado"].ToString();
            desafio.Area = dr["area"].ToString();
            desafio.Nivel = dr["nivel"].ToString();
            desafio.Gabarito = Converte.Texto(dr, "gabarito");
            desafio.MinutosRecompensa = Convert.ToInt32(dr["minutosRecompensa"]);
            return desafio;
        }
    }
}
