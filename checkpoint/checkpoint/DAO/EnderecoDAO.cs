using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class EnderecoDAO : IDao<Endereco>
    {
        private const string SelectBase = @"SELECT en.*, b.nome AS nomeBairro, b.FK_CidadeId,
                ci.nome AS nomeCidade, ci.FK_EstadoId, es.uf
            FROM endereco en
            INNER JOIN bairro b ON b.bairroId = en.FK_BairroId
            INNER JOIN cidade ci ON ci.cidadeId = b.FK_CidadeId
            INNER JOIN estado es ON es.estadoId = ci.FK_EstadoId";

        public void Insert(Endereco endereco)
        {
            string sql = @"INSERT INTO endereco (logradouro, numero, complemento, cep, referencia, FK_BairroId)
                VALUES (@logradouro, @numero, @complemento, @cep, @referencia, @bairroId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, endereco);
                cmd.ExecuteNonQuery();
                endereco.EnderecoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Endereco endereco)
        {
            string sql = @"UPDATE endereco SET logradouro = @logradouro, numero = @numero, complemento = @complemento,
                cep = @cep, referencia = @referencia, FK_BairroId = @bairroId WHERE enderecoId = @enderecoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, endereco);
                cmd.Parameters.AddWithValue("@enderecoId", endereco.EnderecoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum endereço foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM endereco WHERE enderecoId = @enderecoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@enderecoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum endereço encontrado com esse ID.");
            }
        }

        public List<Endereco> List()
        {
            List<Endereco> enderecos = new List<Endereco>();
            string sql = SelectBase + " ORDER BY ci.nome, b.nome, en.logradouro";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    enderecos.Add(Mapear(dr));
            }
            return enderecos;
        }

        public Endereco BuscarID(int id)
        {
            string sql = SelectBase + " WHERE en.enderecoId = @enderecoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@enderecoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum endereço encontrado com esse ID.");
                }
            }
        }

        private void Parametros(MySqlCommand cmd, Endereco endereco)
        {
            cmd.Parameters.AddWithValue("@logradouro", endereco.Logradouro);
            cmd.Parameters.AddWithValue("@numero", Converte.ParaBanco(endereco.Numero));
            cmd.Parameters.AddWithValue("@complemento", Converte.ParaBanco(endereco.Complemento));
            cmd.Parameters.AddWithValue("@cep", endereco.Cep);
            cmd.Parameters.AddWithValue("@referencia", Converte.ParaBanco(endereco.Referencia));
            cmd.Parameters.AddWithValue("@bairroId", endereco.Bairro.BairroId);
        }

        private Endereco Mapear(MySqlDataReader dr)
        {
            Endereco endereco = new Endereco();
            endereco.EnderecoId = Convert.ToInt32(dr["enderecoId"]);
            endereco.Logradouro = dr["logradouro"].ToString();
            endereco.Numero = Converte.Texto(dr, "numero");
            endereco.Complemento = Converte.Texto(dr, "complemento");
            endereco.Cep = dr["cep"].ToString();
            endereco.Referencia = Converte.Texto(dr, "referencia");
            endereco.Bairro = new Bairro();
            endereco.Bairro.BairroId = Convert.ToInt32(dr["FK_BairroId"]);
            endereco.Bairro.Nome = dr["nomeBairro"].ToString();
            endereco.Bairro.Cidade = new Cidade();
            endereco.Bairro.Cidade.CidadeId = Convert.ToInt32(dr["FK_CidadeId"]);
            endereco.Bairro.Cidade.Nome = dr["nomeCidade"].ToString();
            endereco.Bairro.Cidade.Estado = new Estado();
            endereco.Bairro.Cidade.Estado.EstadoId = Convert.ToInt32(dr["FK_EstadoId"]);
            endereco.Bairro.Cidade.Estado.Uf = dr["uf"].ToString();
            return endereco;
        }
    }
}
