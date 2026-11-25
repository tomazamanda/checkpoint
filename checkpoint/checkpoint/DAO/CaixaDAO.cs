using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class CaixaDAO : ICaixaDAO
    {
        private const string SelectBase = @"SELECT cx.*, p.nome AS nomeFuncionario
            FROM caixa cx
            INNER JOIN funcionario f ON f.funcionarioId = cx.FK_FuncionarioId
            INNER JOIN pessoa p ON p.pessoaId = f.FK_PessoaId";

        public void Insert(Caixa caixa)
        {
            string sql = @"INSERT INTO caixa (dataHoraAbertura, valorAbertura, status, FK_FuncionarioId)
                VALUES (@dataHoraAbertura, @valorAbertura, @status, @funcionarioId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@dataHoraAbertura", caixa.DataHoraAbertura);
                cmd.Parameters.AddWithValue("@valorAbertura", caixa.ValorAbertura);
                cmd.Parameters.AddWithValue("@status", caixa.Status);
                cmd.Parameters.AddWithValue("@funcionarioId", caixa.Funcionario.FuncionarioId);
                cmd.ExecuteNonQuery();
                caixa.CaixaId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Caixa caixa)
        {
            string sql = @"UPDATE caixa SET valorAbertura = @valorAbertura, dataHoraFechamento = @dataHoraFechamento,
                valorFechamento = @valorFechamento, status = @status WHERE caixaId = @caixaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@valorAbertura", caixa.ValorAbertura);
                cmd.Parameters.AddWithValue("@dataHoraFechamento", Converte.ParaBanco(caixa.DataHoraFechamento));
                cmd.Parameters.AddWithValue("@valorFechamento", Converte.ParaBanco(caixa.ValorFechamento));
                cmd.Parameters.AddWithValue("@status", caixa.Status);
                cmd.Parameters.AddWithValue("@caixaId", caixa.CaixaId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum caixa foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM caixa WHERE caixaId = @caixaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@caixaId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum caixa encontrado com esse ID.");
            }
        }

        public List<Caixa> List()
        {
            List<Caixa> caixas = new List<Caixa>();
            string sql = SelectBase + " ORDER BY cx.dataHoraAbertura DESC";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    caixas.Add(Mapear(dr));
            }
            return caixas;
        }

        public Caixa BuscarID(int id)
        {
            string sql = SelectBase + " WHERE cx.caixaId = @caixaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@caixaId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum caixa encontrado com esse ID.");
                }
            }
        }

        // Devolve null quando não há caixa aberto
        public Caixa BuscarAberto()
        {
            string sql = SelectBase + " WHERE cx.status = 'aberto' LIMIT 1";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                    return Mapear(dr);
                return null;
            }
        }

        // COALESCE: caixa sem nenhuma comanda fechada soma 0, e não NULL (ver ClienteDAO.SaldoMinutosSociais)
        public double TotalComandasFechadas(int caixaId)
        {
            string sql = "SELECT COALESCE(SUM(valorTotal), 0) FROM comanda WHERE FK_CaixaId = @caixaId AND status = 'fechada'";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@caixaId", caixaId);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    return Convert.ToDouble(dr[0]);   // a consulta devolve um número só
                }
            }
        }

        private Caixa Mapear(MySqlDataReader dr)
        {
            Caixa caixa = new Caixa();
            caixa.CaixaId = Convert.ToInt32(dr["caixaId"]);
            caixa.DataHoraAbertura = Convert.ToDateTime(dr["dataHoraAbertura"]);
            caixa.ValorAbertura = Convert.ToDouble(dr["valorAbertura"]);
            caixa.DataHoraFechamento = Converte.DataHora(dr, "dataHoraFechamento");
            caixa.ValorFechamento = Converte.Numero(dr, "valorFechamento");
            caixa.Status = dr["status"].ToString();
            caixa.Funcionario = new Funcionario();
            caixa.Funcionario.FuncionarioId = Convert.ToInt32(dr["FK_FuncionarioId"]);
            caixa.Funcionario.Nome = dr["nomeFuncionario"].ToString();
            return caixa;
        }
    }
}
