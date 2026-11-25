using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ComandaDAO : IComandaDAO
    {
        private const string SelectBase = @"SELECT co.*, p.nome AS nomeCliente
            FROM comanda co
            LEFT JOIN cliente c ON c.clienteId = co.FK_ClienteId
            LEFT JOIN pessoa p ON p.pessoaId = c.FK_PessoaId";

        public void Insert(Comanda comanda)
        {
            string sql = @"INSERT INTO comanda (dataHoraAbertura, status, FK_ClienteId, FK_CaixaId)
                VALUES (@dataHoraAbertura, @status, @clienteId, @caixaId)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@dataHoraAbertura", comanda.DataHoraAbertura);
                cmd.Parameters.AddWithValue("@status", comanda.Status);
                if (comanda.Cliente == null)   // comanda sem cliente cadastrado
                    cmd.Parameters.AddWithValue("@clienteId", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@clienteId", comanda.Cliente.ClienteId);
                cmd.Parameters.AddWithValue("@caixaId", comanda.Caixa.CaixaId);
                cmd.ExecuteNonQuery();
                comanda.ComandaId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Comanda comanda)
        {
            string sql = @"UPDATE comanda SET dataHoraFechamento = @dataHoraFechamento, valorTotal = @valorTotal,
                formaPagamento = @formaPagamento, status = @status, FK_ClienteId = @clienteId
                WHERE comandaId = @comandaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@dataHoraFechamento", Converte.ParaBanco(comanda.DataHoraFechamento));
                cmd.Parameters.AddWithValue("@valorTotal", Converte.ParaBanco(comanda.ValorTotal));
                cmd.Parameters.AddWithValue("@formaPagamento", Converte.ParaBanco(comanda.FormaPagamento));
                cmd.Parameters.AddWithValue("@status", comanda.Status);
                if (comanda.Cliente == null)   // comanda sem cliente cadastrado
                    cmd.Parameters.AddWithValue("@clienteId", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@clienteId", comanda.Cliente.ClienteId);
                cmd.Parameters.AddWithValue("@comandaId", comanda.ComandaId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma comanda foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM comanda WHERE comandaId = @comandaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@comandaId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma comanda encontrada com esse ID.");
            }
        }

        public List<Comanda> List()
        {
            return Listar(SelectBase + " ORDER BY co.dataHoraAbertura DESC");
        }

        public List<Comanda> ListarAbertas()
        {
            return Listar(SelectBase + " WHERE co.status = 'aberta' ORDER BY co.dataHoraAbertura");
        }

        // Busca a comanda completa: com os itens e as sessões dela
        public Comanda BuscarID(int id)
        {
            Comanda comanda = null;
            string sql = SelectBase + " WHERE co.comandaId = @comandaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@comandaId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        comanda = Mapear(dr);
                }
            }

            if (comanda == null)
                throw new Exception("Nenhuma comanda encontrada com esse ID.");

            comanda.Itens = new ItemComandaDAO().ListarPorComanda(id);
            comanda.Sessoes = new SessaoDAO().ListarPorComanda(id);
            return comanda;
        }

        private List<Comanda> Listar(string sql)
        {
            List<Comanda> comandas = new List<Comanda>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    comandas.Add(Mapear(dr));
            }
            return comandas;
        }

        private Comanda Mapear(MySqlDataReader dr)
        {
            Comanda comanda = new Comanda();
            comanda.ComandaId = Convert.ToInt32(dr["comandaId"]);
            comanda.DataHoraAbertura = Convert.ToDateTime(dr["dataHoraAbertura"]);
            comanda.DataHoraFechamento = Converte.DataHora(dr, "dataHoraFechamento");
            comanda.ValorTotal = Converte.Numero(dr, "valorTotal");
            comanda.FormaPagamento = Converte.Texto(dr, "formaPagamento");
            comanda.Status = dr["status"].ToString();
            comanda.Caixa = new Caixa();
            comanda.Caixa.CaixaId = Convert.ToInt32(dr["FK_CaixaId"]);

            int? clienteId = Converte.Inteiro(dr, "FK_ClienteId");
            if (clienteId != null)
            {
                comanda.Cliente = new Cliente();
                comanda.Cliente.ClienteId = clienteId.Value;
                comanda.Cliente.Nome = dr["nomeCliente"].ToString();
            }

            return comanda;
        }
    }
}
