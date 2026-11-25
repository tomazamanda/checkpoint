using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class BairroDAO : IDao<Bairro>
    {
        private const string SelectBase = @"SELECT b.*, ci.nome AS nomeCidade
            FROM bairro b
            INNER JOIN cidade ci ON ci.cidadeId = b.FK_CidadeId";

        public void Insert(Bairro bairro)
        {
            string sql = "INSERT INTO bairro (nome, FK_CidadeId) VALUES (@nome, @cidadeId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", bairro.Nome);
                cmd.Parameters.AddWithValue("@cidadeId", bairro.Cidade.CidadeId);
                cmd.ExecuteNonQuery();
                bairro.BairroId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Bairro bairro)
        {
            string sql = "UPDATE bairro SET nome = @nome, FK_CidadeId = @cidadeId WHERE bairroId = @bairroId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", bairro.Nome);
                cmd.Parameters.AddWithValue("@cidadeId", bairro.Cidade.CidadeId);
                cmd.Parameters.AddWithValue("@bairroId", bairro.BairroId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum bairro foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM bairro WHERE bairroId = @bairroId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@bairroId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum bairro encontrado com esse ID.");
            }
        }

        public List<Bairro> List()
        {
            List<Bairro> bairros = new List<Bairro>();
            string sql = SelectBase + " ORDER BY ci.nome, b.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    bairros.Add(Mapear(dr));
            }
            return bairros;
        }

        public Bairro BuscarID(int id)
        {
            string sql = SelectBase + " WHERE b.bairroId = @bairroId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@bairroId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum bairro encontrado com esse ID.");
                }
            }
        }

        private Bairro Mapear(MySqlDataReader dr)
        {
            Bairro bairro = new Bairro();
            bairro.BairroId = Convert.ToInt32(dr["bairroId"]);
            bairro.Nome = dr["nome"].ToString();
            bairro.Cidade = new Cidade();
            bairro.Cidade.CidadeId = Convert.ToInt32(dr["FK_CidadeId"]);
            bairro.Cidade.Nome = dr["nomeCidade"].ToString();
            return bairro;
        }
    }
}
