using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ClienteDAO : IClienteDAO
    {
        private const string SelectBase = @"SELECT c.*, p.*, pr.nome AS nomeResponsavel, pr.telefone AS telefoneResponsavel
            FROM cliente c
            INNER JOIN pessoa p ON p.pessoaId = c.FK_PessoaId
            LEFT JOIN responsavel r ON r.responsavelId = c.FK_ResponsavelId
            LEFT JOIN pessoa pr ON pr.pessoaId = r.FK_PessoaId";

        public void Insert(Cliente cliente)
        {
            cliente.ValidarResponsavel();

            using (var conexao = Conexao.Conectar())
            // Pessoa e cliente são gravados juntos numa transação (ver ItemComandaDAO.Insert).
            using (var transacao = conexao.BeginTransaction())
            {
                if (cliente.PessoaId == 0)
                    PessoaDAO.Insert(cliente, conexao, transacao);

                string sql = @"INSERT INTO cliente (dataCadastro, parentesco, dataAutorizacao, FK_PessoaId, FK_ResponsavelId)
                    VALUES (@dataCadastro, @parentesco, @dataAutorizacao, @pessoaId, @responsavelId)";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    Parametros(cmd, cliente);
                    cmd.Parameters.AddWithValue("@pessoaId", cliente.PessoaId);
                    cmd.ExecuteNonQuery();
                    cliente.ClienteId = (int)cmd.LastInsertedId;
                }
                transacao.Commit();
            }
        }

        public void Update(Cliente cliente)
        {
            cliente.ValidarResponsavel();

            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                PessoaDAO.Update(cliente, conexao, transacao);

                string sql = @"UPDATE cliente SET dataCadastro = @dataCadastro, parentesco = @parentesco,
                    dataAutorizacao = @dataAutorizacao, FK_ResponsavelId = @responsavelId WHERE clienteId = @clienteId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    Parametros(cmd, cliente);
                    cmd.Parameters.AddWithValue("@clienteId", cliente.ClienteId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Nenhum cliente foi atualizado (verifique o ID).");
                }
                transacao.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                int pessoaId = PessoaDAO.PessoaIdDe("cliente", "clienteId", id, conexao, transacao);

                string sql = "DELETE FROM cliente WHERE clienteId = @clienteId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@clienteId", id);
                    cmd.ExecuteNonQuery();
                }
                PessoaDAO.DeleteSeNaoUsada(pessoaId, conexao, transacao);
                transacao.Commit();
            }
        }

        public List<Cliente> List()
        {
            List<Cliente> clientes = new List<Cliente>();
            string sql = SelectBase + " ORDER BY p.nome";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    clientes.Add(Mapear(dr));
            }
            return clientes;
        }

        public Cliente BuscarID(int id)
        {
            string sql = SelectBase + " WHERE c.clienteId = @clienteId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@clienteId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum cliente encontrado com esse ID.");
                }
            }
        }

        /* Saldo = minutos ganhos (resumos + resoluções aprovados) - minutos usados em sessões por crédito social.
          Pesquisei para estudar: COALESCE(valor, 0) troca NULL por 0. Sem nenhuma linha, o SUM devolve NULL, e NULL + 45 dá NULL, então o saldo daria erro no Convert.ToInt32. */
        public int SaldoMinutosSociais(int clienteId)
        {
            string sql = @"SELECT
                (SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resumo WHERE FK_ClienteId = @clienteId AND status = 'aprovado')
              + (SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resolucao WHERE FK_ClienteId = @clienteId AND status = 'aprovado')
              - (SELECT COALESCE(SUM(minutos), 0) FROM sessao WHERE FK_ClienteId = @clienteId AND origem = 'credito social')";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    return Convert.ToInt32(dr[0]);   // a consulta devolve um número só
                }
            }
        }

        /* Minutos aprovados (resumos + resoluções) entregues na mesma semana do dia informado.
           Pesquisei para estudar: YEARWEEK(data, 1) devolve o ano junto com o número da semana, começando na segunda. Duas datas da mesma semana dão o mesmo número. */
        public int MinutosGanhosNaSemana(int clienteId, DateTime dia)
        {
            string sql = @"SELECT
                (SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resumo WHERE FK_ClienteId = @clienteId
                    AND status = 'aprovado' AND YEARWEEK(dataHoraEntrega, 1) = YEARWEEK(@dia, 1))
              + (SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resolucao WHERE FK_ClienteId = @clienteId
                    AND status = 'aprovado' AND YEARWEEK(dataHoraEntrega, 1) = YEARWEEK(@dia, 1))";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.Parameters.AddWithValue("@dia", dia);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    return Convert.ToInt32(dr[0]);   // a consulta devolve um número só
                }
            }
        }

        private void Parametros(MySqlCommand cmd, Cliente cliente)
        {
            cmd.Parameters.AddWithValue("@dataCadastro", Converte.ParaBanco(cliente.DataCadastro));
            cmd.Parameters.AddWithValue("@parentesco", Converte.ParaBanco(cliente.Parentesco));
            cmd.Parameters.AddWithValue("@dataAutorizacao", Converte.ParaBanco(cliente.DataAutorizacao));
            if (cliente.Responsavel == null)   // cliente adulto não tem responsável
                cmd.Parameters.AddWithValue("@responsavelId", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@responsavelId", cliente.Responsavel.ResponsavelId);
        }

        private Cliente Mapear(MySqlDataReader dr)
        {
            Cliente cliente = new Cliente();
            PessoaDAO.Mapear(dr, cliente);
            cliente.ClienteId = Convert.ToInt32(dr["clienteId"]);
            cliente.DataCadastro = DateOnly.FromDateTime(Convert.ToDateTime(dr["dataCadastro"]));
            cliente.Parentesco = Converte.Texto(dr, "parentesco");
            cliente.DataAutorizacao = Converte.Data(dr, "dataAutorizacao");

            int? responsavelId = Converte.Inteiro(dr, "FK_ResponsavelId");
            if (responsavelId != null)
            {
                cliente.Responsavel = new Responsavel();
                cliente.Responsavel.ResponsavelId = responsavelId.Value;
                cliente.Responsavel.Nome = dr["nomeResponsavel"].ToString();
                cliente.Responsavel.Telefone = Converte.Texto(dr, "telefoneResponsavel");
            }
            return cliente;
        }
    }
}
