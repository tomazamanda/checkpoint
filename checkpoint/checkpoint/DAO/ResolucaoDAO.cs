using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ResolucaoDAO : IAtividadeSocialDAO<Resolucao>
    {
        private const string SelectBase = @"SELECT r.*, d.titulo, d.area, d.nivel, d.minutosRecompensa,
                p.nome AS nomeCliente
            FROM resolucao r
            INNER JOIN desafio d ON d.desafioId = r.FK_DesafioId
            INNER JOIN cliente c ON c.clienteId = r.FK_ClienteId
            INNER JOIN pessoa p ON p.pessoaId = c.FK_PessoaId";

        public void Insert(Resolucao resolucao)
        {
            string sql = @"INSERT INTO resolucao (dataHoraInicio, dataHoraEntrega, fotoUrl, status, minutosConcedidos,
                    observacao, FK_ClienteId, FK_DesafioId, FK_FuncionarioId)
                VALUES (@dataHoraInicio, @dataHoraEntrega, @fotoUrl, @status, @minutosConcedidos,
                    @observacao, @clienteId, @desafioId, @funcionarioId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                AtividadeSocialDAO.Parametros(cmd, resolucao);
                cmd.Parameters.AddWithValue("@desafioId", resolucao.Desafio.DesafioId);
                cmd.ExecuteNonQuery();
                resolucao.ResolucaoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Resolucao resolucao)
        {
            string sql = @"UPDATE resolucao SET dataHoraInicio = @dataHoraInicio, dataHoraEntrega = @dataHoraEntrega,
                fotoUrl = @fotoUrl, status = @status, minutosConcedidos = @minutosConcedidos, observacao = @observacao,
                FK_ClienteId = @clienteId, FK_DesafioId = @desafioId, FK_FuncionarioId = @funcionarioId
                WHERE resolucaoId = @resolucaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                AtividadeSocialDAO.Parametros(cmd, resolucao);
                cmd.Parameters.AddWithValue("@desafioId", resolucao.Desafio.DesafioId);
                cmd.Parameters.AddWithValue("@resolucaoId", resolucao.ResolucaoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma resolução foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM resolucao WHERE resolucaoId = @resolucaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@resolucaoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma resolução encontrada com esse ID.");
            }
        }

        public List<Resolucao> List()
        {
            return Listar(SelectBase + " ORDER BY r.dataHoraInicio DESC");
        }

        public List<Resolucao> ListarEntregues()
        {
            return Listar(SelectBase + " WHERE r.status = 'entregue' ORDER BY r.dataHoraEntrega");
        }

        public Resolucao BuscarID(int id)
        {
            string sql = SelectBase + " WHERE r.resolucaoId = @resolucaoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@resolucaoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma resolução encontrada com esse ID.");
                }
            }
        }

        private List<Resolucao> Listar(string sql)
        {
            List<Resolucao> resolucoes = new List<Resolucao>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    resolucoes.Add(Mapear(dr));
            }
            return resolucoes;
        }

        private Resolucao Mapear(MySqlDataReader dr)
        {
            Resolucao resolucao = new Resolucao();
            AtividadeSocialDAO.Mapear(dr, resolucao);
            resolucao.ResolucaoId = Convert.ToInt32(dr["resolucaoId"]);
            resolucao.Desafio = new Desafio();
            resolucao.Desafio.DesafioId = Convert.ToInt32(dr["FK_DesafioId"]);
            resolucao.Desafio.Titulo = dr["titulo"].ToString();
            resolucao.Desafio.Area = dr["area"].ToString();
            resolucao.Desafio.Nivel = dr["nivel"].ToString();
            resolucao.Desafio.MinutosRecompensa = Convert.ToInt32(dr["minutosRecompensa"]);
            return resolucao;
        }
    }
}
