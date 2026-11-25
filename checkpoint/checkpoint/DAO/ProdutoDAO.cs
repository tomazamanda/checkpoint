using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ProdutoDAO : IProdutoDAO
    {
        public void Insert(Produto produto)
        {
            string sql = @"INSERT INTO produto (nome, tipo, unidade, precoVenda, qtdEstoque, qtdMinima)
                VALUES (@nome, @tipo, @unidade, @precoVenda, @qtdEstoque, @qtdMinima)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, produto);
                cmd.ExecuteNonQuery();
                produto.ProdutoId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Produto produto)
        {
            string sql = @"UPDATE produto SET nome = @nome, tipo = @tipo, unidade = @unidade, precoVenda = @precoVenda,
                qtdEstoque = @qtdEstoque, qtdMinima = @qtdMinima WHERE produtoId = @produtoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, produto);
                cmd.Parameters.AddWithValue("@produtoId", produto.ProdutoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum produto foi atualizado (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM produto WHERE produtoId = @produtoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@produtoId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum produto encontrado com esse ID.");
            }
        }

        public List<Produto> List()
        {
            return Listar("SELECT * FROM produto ORDER BY tipo DESC, nome");
        }

        public List<Produto> EstoqueBaixo()
        {
            return Listar("SELECT * FROM produto WHERE qtdEstoque <= qtdMinima ORDER BY nome");
        }

        // Soma direto no banco (qtdEstoque + @quantidade), sem ler antes: se uma venda baixar o estoque ao mesmo tempo, as duas contas valem
        public void ReporEstoque(int produtoId, int quantidade)
        {
            string sql = "UPDATE produto SET qtdEstoque = qtdEstoque + @quantidade WHERE produtoId = @produtoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@quantidade", quantidade);
                cmd.Parameters.AddWithValue("@produtoId", produtoId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhum produto encontrado com esse ID.");
            }
        }

        public Produto BuscarID(int id)
        {
            string sql = "SELECT * FROM produto WHERE produtoId = @produtoId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@produtoId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum produto encontrado com esse ID.");
                }
            }
        }

        private List<Produto> Listar(string sql)
        {
            List<Produto> produtos = new List<Produto>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    produtos.Add(Mapear(dr));
            }
            return produtos;
        }

        private void Parametros(MySqlCommand cmd, Produto produto)
        {
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@tipo", produto.Tipo);
            cmd.Parameters.AddWithValue("@unidade", produto.Unidade);
            cmd.Parameters.AddWithValue("@precoVenda", Converte.ParaBanco(produto.PrecoVenda));
            cmd.Parameters.AddWithValue("@qtdEstoque", produto.QtdEstoque);
            cmd.Parameters.AddWithValue("@qtdMinima", produto.QtdMinima);
        }

        private Produto Mapear(MySqlDataReader dr)
        {
            Produto produto = new Produto();
            produto.ProdutoId = Convert.ToInt32(dr["produtoId"]);
            produto.Nome = dr["nome"].ToString();
            produto.Tipo = dr["tipo"].ToString();
            produto.Unidade = dr["unidade"].ToString();
            produto.PrecoVenda = Converte.Numero(dr, "precoVenda");
            produto.QtdEstoque = Convert.ToInt32(dr["qtdEstoque"]);
            produto.QtdMinima = Convert.ToInt32(dr["qtdMinima"]);
            return produto;
        }
    }
}
