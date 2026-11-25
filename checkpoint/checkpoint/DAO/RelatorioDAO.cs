using checkpoint.Database;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    // Consultas com GROUP BY (só leitura). Cada consulta devolve as colunas descricao, quantidade e total, para caber na classe LinhaRelatorio.
    public class RelatorioDAO
    {
        public List<LinhaRelatorio> FaturamentoPorFormaPagamento()
        {
            return Executar(@"SELECT formaPagamento AS descricao, COUNT(*) AS quantidade, SUM(valorTotal) AS total
                FROM comanda
                WHERE status = 'fechada'
                GROUP BY formaPagamento
                ORDER BY total DESC");
        }

        // COALESCE: sessão em andamento ainda não tem valor (NULL); se um tipo só tiver essas, o total vira 0
        public List<LinhaRelatorio> SessoesPorTipoEstacao()
        {
            return Executar(@"SELECT e.tipo AS descricao, COUNT(*) AS quantidade, COALESCE(SUM(s.valor), 0) AS total
                FROM sessao s
                INNER JOIN estacao e ON e.estacaoId = s.FK_EstacaoId
                GROUP BY e.tipo");
        }

        public List<LinhaRelatorio> ProdutosMaisVendidos()
        {
            return Executar(@"SELECT pr.nome AS descricao, SUM(i.quantidade) AS quantidade,
                    SUM(i.quantidade * i.precoUnitario) AS total
                FROM itemComanda i
                INNER JOIN produto pr ON pr.produtoId = i.FK_ProdutoId
                GROUP BY i.FK_ProdutoId, pr.nome
                ORDER BY quantidade DESC
                LIMIT 10");
        }

        private List<LinhaRelatorio> Executar(string sql)
        {
            List<LinhaRelatorio> linhas = new List<LinhaRelatorio>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    LinhaRelatorio linha = new LinhaRelatorio();
                    linha.Descricao = dr["descricao"].ToString();
                    linha.Quantidade = Convert.ToInt32(dr["quantidade"]);
                    linha.Total = Convert.ToDouble(dr["total"]);
                    linhas.Add(linha);
                }
            }
            return linhas;
        }
    }
}
