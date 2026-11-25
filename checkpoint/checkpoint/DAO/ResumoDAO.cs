using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ResumoDAO : IAtividadeSocialDAO<Resumo>
    {
        private const string SelectBase = @"SELECT r.*, o.titulo, o.tipo, o.faixaEtaria, o.minutosRecompensa,
                p.nome AS nomeCliente
            FROM resumo r
            INNER JOIN obra o ON o.obraId = r.FK_ObraId
            INNER JOIN cliente c ON c.clienteId = r.FK_ClienteId
            INNER JOIN pessoa p ON p.pessoaId = c.FK_PessoaId";

        public void Insert(Resumo resumo)
        {
            string sql = @"INSERT INTO resumo (dataHoraInicio, dataHoraEntrega, fotoUrl, status, minutosConcedidos,
                    observacao, FK_ClienteId, FK_ObraId, FK_FuncionarioId)
                VALUES (@dataHoraInicio, @dataHoraEntrega, @fotoUrl, @status, @minutosConcedidos,
                    @observacao, @clienteId, @obraId, @funcionarioId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                AtividadeSocialDAO.Parametros(cmd, resumo);
                cmd.Parameters.AddWithValue("@obraId", resumo.Obra.ObraId);
                cmd.ExecuteNonQuery();
                resumo.ResumoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Resumo resumo)
        {
            string sql = @"UPDATE resumo SET dataHoraInicio = @dataHoraInicio, dataHoraEntrega = @dataHoraEntrega,
                fotoUrl = @fotoUrl, status = @status, minutosConcedidos = @minutosConcedidos, observacao = @observacao,
                FK_ClienteId = @clienteId, FK_ObraId = @obraId, FK_FuncionarioId = @funcionarioId
                WHERE resumoId = @resumoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                AtividadeSocialDAO.Parametros(cmd, resumo);
                cmd.Parameters.AddWithValue("@obraId", resumo.Obra.ObraId);
                cmd.Parameters.AddWithValue("@resumoId", resumo.ResumoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum resumo foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM resumo WHERE resumoId = @resumoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@resumoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum resumo encontrado com esse ID.");
            }
        }

        public List<Resumo> List()
        {
            return Listar(SelectBase + " ORDER BY r.dataHoraInicio DESC");
        }

        public List<Resumo> ListarEntregues()
        {
            return Listar(SelectBase + " WHERE r.status = 'entregue' ORDER BY r.dataHoraEntrega");
        }

        public Resumo BuscarID(int id)
        {
            string sql = SelectBase + " WHERE r.resumoId = @resumoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@resumoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum resumo encontrado com esse ID.");
                }
            }
        }

        private List<Resumo> Listar(string sql)
        {
            List<Resumo> resumos = new List<Resumo>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    resumos.Add(Mapear(dr));
            }
            return resumos;
        }

        private Resumo Mapear(MySqlDataReader dr)
        {
            Resumo resumo = new Resumo();
            AtividadeSocialDAO.Mapear(dr, resumo);
            resumo.ResumoId = Convert.ToInt32(dr["resumoId"]);
            resumo.Obra = new Obra();
            resumo.Obra.ObraId = Convert.ToInt32(dr["FK_ObraId"]);
            resumo.Obra.Titulo = dr["titulo"].ToString();
            resumo.Obra.Tipo = dr["tipo"].ToString();
            resumo.Obra.FaixaEtaria = Convert.ToInt32(dr["faixaEtaria"]);
            resumo.Obra.MinutosRecompensa = Convert.ToInt32(dr["minutosRecompensa"]);
            return resumo;
        }
    }
}
