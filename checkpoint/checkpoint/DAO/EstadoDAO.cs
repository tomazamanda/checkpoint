using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class EstadoDAO : IDao<Estado>
    {
        public void Insert(Estado estado)
        {
            string sql = "INSERT INTO estado (nome, uf) VALUES (@nome, @uf)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", estado.Nome);
                cmd.Parameters.AddWithValue("@uf", estado.Uf);
                cmd.ExecuteNonQuery();
                estado.EstadoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Estado estado)
        {
            string sql = "UPDATE estado SET nome = @nome, uf = @uf WHERE estadoId = @estadoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", estado.Nome);
                cmd.Parameters.AddWithValue("@uf", estado.Uf);
                cmd.Parameters.AddWithValue("@estadoId", estado.EstadoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum estado foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM estado WHERE estadoId = @estadoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@estadoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum estado encontrado com esse ID.");
            }
        }

        public List<Estado> List()
        {
            List<Estado> estados = new List<Estado>();
            string sql = "SELECT * FROM estado ORDER BY nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    estados.Add(Mapear(dr));
            }
            return estados;
        }

        public Estado BuscarID(int id)
        {
            string sql = "SELECT * FROM estado WHERE estadoId = @estadoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@estadoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum estado encontrado com esse ID.");
                }
            }
        }

        private Estado Mapear(MySqlDataReader dr)
        {
            Estado estado = new Estado();
            estado.EstadoId = Convert.ToInt32(dr["estadoId"]);
            estado.Nome = dr["nome"].ToString();
            estado.Uf = dr["uf"].ToString();
            return estado;
        }
    }
}
