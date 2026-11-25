using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class SessaoDAO : ISessaoDAO
    {
        private const string SelectBase = @"SELECT s.*, e.numero, e.tipo, e.plataforma, e.status AS statusEstacao,
                e.precoHora, p.nome AS nomeCliente
            FROM sessao s
            INNER JOIN estacao e ON e.estacaoId = s.FK_EstacaoId
            INNER JOIN cliente c ON c.clienteId = s.FK_ClienteId
            INNER JOIN pessoa p ON p.pessoaId = c.FK_PessoaId";

        // Iniciar sessão = ocupar a estação E gravar a sessão, juntos em uma transação
        public void Insert(Sessao sessao)
        {
            sessao.Validar();

            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                /* Só ocupa a estação se não houver sessão aberta nela.
                   Pesquisei para estudar: NOT EXISTS numa subconsulta (não é o IF NOT EXISTS do CREATE TABLE). Ele é verdadeiro quando a subconsulta não acha nenhuma linha. */
                string sqlEstacao = @"UPDATE estacao SET status = 'em uso'
                    WHERE estacaoId = @estacaoId AND status = 'disponivel'
                    AND NOT EXISTS (SELECT 1 FROM sessao WHERE FK_EstacaoId = @estacaoId AND dataHoraFim IS NULL)";
                using (var cmd = new MySqlCommand(sqlEstacao, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@estacaoId", sessao.Estacao.EstacaoId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Esta estação não está disponível.");
                }

                string sql = @"INSERT INTO sessao (dataHoraInicio, origem, FK_ClienteId, FK_EstacaoId, FK_ComandaId)
                    VALUES (@dataHoraInicio, @origem, @clienteId, @estacaoId, @comandaId)";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@dataHoraInicio", sessao.DataHoraInicio);
                    cmd.Parameters.AddWithValue("@origem", sessao.Origem);
                    cmd.Parameters.AddWithValue("@clienteId", sessao.Cliente.ClienteId);
                    cmd.Parameters.AddWithValue("@estacaoId", sessao.Estacao.EstacaoId);
                    if (sessao.Comanda == null)   // sessão por crédito social não fica em comanda
                        cmd.Parameters.AddWithValue("@comandaId", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@comandaId", sessao.Comanda.ComandaId);
                    cmd.ExecuteNonQuery();
                    sessao.SessaoId = (int)cmd.LastInsertedId;
                }
                transacao.Commit();
                sessao.Estacao.Status = "em uso";
            }
        }

        // Grava o fim da sessão; se ela terminou, libera a estação
        public void Update(Sessao sessao)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                string sql = @"UPDATE sessao SET dataHoraFim = @dataHoraFim, minutos = @minutos, valor = @valor
                    WHERE sessaoId = @sessaoId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@dataHoraFim", Converte.ParaBanco(sessao.DataHoraFim));
                    cmd.Parameters.AddWithValue("@minutos", Converte.ParaBanco(sessao.Minutos));
                    cmd.Parameters.AddWithValue("@valor", Converte.ParaBanco(sessao.Valor));
                    cmd.Parameters.AddWithValue("@sessaoId", sessao.SessaoId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Nenhuma sessão foi atualizada (verifique o ID).");
                }

                if (sessao.DataHoraFim != null)
                {
                    string sqlEstacao = "UPDATE estacao SET status = 'disponivel' WHERE estacaoId = @estacaoId AND status = 'em uso'";
                    using (var cmd = new MySqlCommand(sqlEstacao, conexao, transacao))
                    {
                        cmd.Parameters.AddWithValue("@estacaoId", sessao.Estacao.EstacaoId);
                        cmd.ExecuteNonQuery();
                    }
                }
                transacao.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                // se a sessão ainda estava aberta, a estação volta a ficar disponível, pesquisei para estudar: UPDATE com INNER JOIN (ver ItemComandaDAO.Update).
                string sqlEstacao = @"UPDATE estacao e
                    INNER JOIN sessao s ON s.FK_EstacaoId = e.estacaoId
                    SET e.status = 'disponivel'
                    WHERE s.sessaoId = @sessaoId AND s.dataHoraFim IS NULL AND e.status = 'em uso'";
                using (var cmd = new MySqlCommand(sqlEstacao, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@sessaoId", id);
                    cmd.ExecuteNonQuery();
                }

                string sql = "DELETE FROM sessao WHERE sessaoId = @sessaoId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@sessaoId", id);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Nenhuma sessão encontrada com esse ID.");
                }
                transacao.Commit();
            }
        }

        public List<Sessao> List()
        {
            return Listar(SelectBase + " ORDER BY s.dataHoraInicio DESC", 0);
        }

        public List<Sessao> ListarAbertas()
        {
            return Listar(SelectBase + " WHERE s.dataHoraFim IS NULL ORDER BY s.dataHoraInicio", 0);
        }

        public List<Sessao> ListarPorComanda(int comandaId)
        {
            return Listar(SelectBase + " WHERE s.FK_ComandaId = @comandaId ORDER BY s.dataHoraInicio", comandaId);
        }

        public Sessao BuscarID(int id)
        {
            string sql = SelectBase + " WHERE s.sessaoId = @sessaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@sessaoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma sessão encontrada com esse ID.");
                }
            }
        }

        private List<Sessao> Listar(string sql, int comandaId)
        {
            List<Sessao> sessoes = new List<Sessao>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@comandaId", comandaId);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        sessoes.Add(Mapear(dr));
                }
            }
            return sessoes;
        }

        private Sessao Mapear(MySqlDataReader dr)
        {
            Sessao sessao = new Sessao();
            sessao.SessaoId = Convert.ToInt32(dr["sessaoId"]);
            sessao.DataHoraInicio = Convert.ToDateTime(dr["dataHoraInicio"]);
            sessao.DataHoraFim = Converte.DataHora(dr, "dataHoraFim");
            sessao.Minutos = Converte.Inteiro(dr, "minutos");
            sessao.Origem = dr["origem"].ToString();
            sessao.Valor = Converte.Numero(dr, "valor");
            sessao.Cliente = new Cliente();
            sessao.Cliente.ClienteId = Convert.ToInt32(dr["FK_ClienteId"]);
            sessao.Cliente.Nome = dr["nomeCliente"].ToString();
            sessao.Estacao = new Estacao();
            sessao.Estacao.EstacaoId = Convert.ToInt32(dr["FK_EstacaoId"]);
            sessao.Estacao.Numero = Convert.ToInt32(dr["numero"]);
            sessao.Estacao.Tipo = dr["tipo"].ToString();
            sessao.Estacao.Plataforma = dr["plataforma"].ToString();
            sessao.Estacao.Status = dr["statusEstacao"].ToString();
            sessao.Estacao.PrecoHora = Convert.ToDouble(dr["precoHora"]);

            int? comandaId = Converte.Inteiro(dr, "FK_ComandaId");
            if (comandaId != null)
            {
                sessao.Comanda = new Comanda();
                sessao.Comanda.ComandaId = comandaId.Value;
            }

            return sessao;
        }
    }
}
