using checkpoint.Database;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    // Pessoa é abstrata: nunca existe "só pessoa". Esse DAO cuida da parte comum (tabela pessoa) e é usado por ClienteDAO, FuncionarioDAO e ResponsavelDAO, sempre dentro da transação deles. 
    public static class PessoaDAO
    {
        public static void Insert(Pessoa pessoa, MySqlConnection conexao, MySqlTransaction transacao)
        {
            string sql = @"INSERT INTO pessoa (nome, cpf, dataNasc, telefone, email, FK_EnderecoId)
                VALUES (@nome, @cpf, @dataNasc, @telefone, @email, @enderecoId)";
            using (var cmd = new MySqlCommand(sql, conexao, transacao))
            {
                Parametros(cmd, pessoa);
                cmd.ExecuteNonQuery();
                // Pesquisei para estudar: LastInsertedId, o ID que o AUTO_INCREMENT acabou de gerar, todos os DAOs usam para preencher o ID do objeto gravado.
                pessoa.PessoaId = (int)cmd.LastInsertedId;
            }
        }

        public static void Update(Pessoa pessoa, MySqlConnection conexao, MySqlTransaction transacao)
        {
            string sql = @"UPDATE pessoa SET nome = @nome, cpf = @cpf, dataNasc = @dataNasc, telefone = @telefone,
                email = @email, FK_EnderecoId = @enderecoId WHERE pessoaId = @pessoaId";
            using (var cmd = new MySqlCommand(sql, conexao, transacao))
            {
                Parametros(cmd, pessoa);
                cmd.Parameters.AddWithValue("@pessoaId", pessoa.PessoaId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma pessoa foi atualizada (verifique o PessoaId).");
            }
        }

        // Só apaga a pessoa se ela não for mais cliente, funcionário nem responsável
        public static void DeleteSeNaoUsada(int pessoaId, MySqlConnection conexao, MySqlTransaction transacao)
        {
            string sql = @"DELETE FROM pessoa WHERE pessoaId = @pessoaId
                AND pessoaId NOT IN (SELECT FK_PessoaId FROM cliente)
                AND pessoaId NOT IN (SELECT FK_PessoaId FROM funcionario)
                AND pessoaId NOT IN (SELECT FK_PessoaId FROM responsavel)";
            using (var cmd = new MySqlCommand(sql, conexao, transacao))
            {
                cmd.Parameters.AddWithValue("@pessoaId", pessoaId);
                cmd.ExecuteNonQuery();
            }
        }

        // tabela e colunaId vêm sempre do código e nunca do usuário, por isso podem ir direto no SQL
        public static int PessoaIdDe(string tabela, string colunaId, int id, MySqlConnection conexao, MySqlTransaction transacao)
        {
            string sql = $"SELECT FK_PessoaId FROM {tabela} WHERE {colunaId} = @id";
            using (var cmd = new MySqlCommand(sql, conexao, transacao))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        throw new Exception("Nenhum registro encontrado com esse ID.");
                    return Convert.ToInt32(dr["FK_PessoaId"]);
                }
            }
        }

        // Preenche a pessoa se o CPF já existir. Serve para uma pessoa que já é responsável (ou funcionário) virar cliente sem duplicar o cadastro.
        public static bool PreencherPorCpf(Pessoa pessoa, string cpf)
        {
            string sql = "SELECT * FROM pessoa WHERE cpf = @cpf";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cpf", cpf);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return false;
                    Mapear(dr, pessoa);
                    return true;
                }
            }
        }

        public static void Mapear(MySqlDataReader dr, Pessoa pessoa)
        {
            pessoa.PessoaId = Convert.ToInt32(dr["pessoaId"]);
            pessoa.Nome = dr["nome"].ToString();
            pessoa.Cpf = dr["cpf"].ToString();
            pessoa.DataNasc = DateOnly.FromDateTime(Convert.ToDateTime(dr["dataNasc"]));
            pessoa.Telefone = Converte.Texto(dr, "telefone");
            pessoa.Email = Converte.Texto(dr, "email");

            int? enderecoId = Converte.Inteiro(dr, "FK_EnderecoId");
            if (enderecoId != null)
            {
                pessoa.Endereco = new Endereco();
                pessoa.Endereco.EnderecoId = enderecoId.Value;
            }
        }

        private static void Parametros(MySqlCommand cmd, Pessoa pessoa)
        {
            cmd.Parameters.AddWithValue("@nome", pessoa.Nome);
            cmd.Parameters.AddWithValue("@cpf", pessoa.Cpf);
            cmd.Parameters.AddWithValue("@dataNasc", Converte.ParaBanco(pessoa.DataNasc));
            cmd.Parameters.AddWithValue("@telefone", Converte.ParaBanco(pessoa.Telefone));
            cmd.Parameters.AddWithValue("@email", Converte.ParaBanco(pessoa.Email));
            if (pessoa.Endereco == null)
                cmd.Parameters.AddWithValue("@enderecoId", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@enderecoId", pessoa.Endereco.EnderecoId);
        }
    }
}
