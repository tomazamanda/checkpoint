using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class CidadeDAO : IDao<Cidade>
    {
        private const string SelectBase = @"SELECT ci.*, es.nome AS nomeEstado, es.uf
            FROM cidade ci
            INNER JOIN estado es ON es.estadoId = ci.FK_EstadoId";

        public void Insert(Cidade cidade)
        {
            string sql = "INSERT INTO cidade (nome, FK_EstadoId) VALUES (@nome, @estadoId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cidade.Nome);
                cmd.Parameters.AddWithValue("@estadoId", cidade.Estado.EstadoId);
                cmd.ExecuteNonQuery();
                cidade.CidadeId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Cidade cidade)
        {
            string sql = "UPDATE cidade SET nome = @nome, FK_EstadoId = @estadoId WHERE cidadeId = @cidadeId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cidade.Nome);
                cmd.Parameters.AddWithValue("@estadoId", cidade.Estado.EstadoId);
                cmd.Parameters.AddWithValue("@cidadeId", cidade.CidadeId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma cidade foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM cidade WHERE cidadeId = @cidadeId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cidadeId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma cidade encontrada com esse ID.");
            }
        }

        public List<Cidade> List()
        {
            List<Cidade> cidades = new List<Cidade>();
            string sql = SelectBase + " ORDER BY ci.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    cidades.Add(Mapear(dr));
            }
            return cidades;
        }

        public Cidade BuscarID(int id)
        {
            string sql = SelectBase + " WHERE ci.cidadeId = @cidadeId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cidadeId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma cidade encontrada com esse ID.");
                }
            }
        }

        private Cidade Mapear(MySqlDataReader dr)
        {
            Cidade cidade = new Cidade();
            cidade.CidadeId = Convert.ToInt32(dr["cidadeId"]);
            cidade.Nome = dr["nome"].ToString();
            cidade.Estado = new Estado();
            cidade.Estado.EstadoId = Convert.ToInt32(dr["FK_EstadoId"]);
            cidade.Estado.Nome = dr["nomeEstado"].ToString();
            cidade.Estado.Uf = dr["uf"].ToString();
            return cidade;
        }
    }
}
