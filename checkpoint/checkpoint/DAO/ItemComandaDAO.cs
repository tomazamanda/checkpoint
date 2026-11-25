using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ItemComandaDAO : IItemComandaDAO
    {
        private const string SelectBase = @"SELECT i.*, pr.nome AS nomeProduto
            FROM itemComanda i
            INNER JOIN produto pr ON pr.produtoId = i.FK_ProdutoId";

        /* Vender = gravar o item E tirar do estoque. Os dois juntos numa transação: se faltar estoque, nada é gravado.
           COMMIT e ROLLBACK foram vistos em BD2. Pesquisei para estudar: fazer a transação pelo C#. BeginTransaction começa, Commit grava tudo, e se der erro antes do Commit nada fica gravado. */
        public void Insert(ItemComanda item)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                string sqlEstoque = @"UPDATE produto SET qtdEstoque = qtdEstoque - @quantidade
                    WHERE produtoId = @produtoId AND qtdEstoque >= @quantidade";
                using (var cmd = new MySqlCommand(sqlEstoque, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmd.Parameters.AddWithValue("@produtoId", item.Produto.ProdutoId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Estoque insuficiente para este produto.");
                }

                /* Se o produto já está na comanda (UNIQUE comanda + produto), soma a quantidade.
                   Pesquisei para estudar: ON DUPLICATE KEY UPDATE, que faz um UPDATE quando o INSERT repetiria o UNIQUE. O LAST_INSERT_ID(itemComandaId) faz o LastInsertedId devolver o ID do item que já existia. */
                string sql = @"INSERT INTO itemComanda (quantidade, precoUnitario, FK_ComandaId, FK_ProdutoId)
                    VALUES (@quantidade, @precoUnitario, @comandaId, @produtoId)
                    ON DUPLICATE KEY UPDATE quantidade = quantidade + @quantidade,
                    itemComandaId = LAST_INSERT_ID(itemComandaId)";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmd.Parameters.AddWithValue("@precoUnitario", item.PrecoUnitario);
                    cmd.Parameters.AddWithValue("@comandaId", item.Comanda.ComandaId);
                    cmd.Parameters.AddWithValue("@produtoId", item.Produto.ProdutoId);
                    cmd.ExecuteNonQuery();
                    item.ItemComandaId = (int)cmd.LastInsertedId;
                }
                transacao.Commit();
            }
        }

        // Muda quantidade/preço e acerta o estoque pela diferença
        public void Update(ItemComanda item)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                // Pesquisei para estudar: UPDATE com INNER JOIN. Muda o estoque do produto usando a quantidade que está no item. O Delete abaixo faz o mesmo. 
                string sqlEstoque = @"UPDATE produto p
                    INNER JOIN itemComanda i ON i.FK_ProdutoId = p.produtoId
                    SET p.qtdEstoque = p.qtdEstoque + i.quantidade - @quantidade
                    WHERE i.itemComandaId = @itemComandaId AND p.qtdEstoque + i.quantidade >= @quantidade";
                using (var cmd = new MySqlCommand(sqlEstoque, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmd.Parameters.AddWithValue("@itemComandaId", item.ItemComandaId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Item não encontrado ou estoque insuficiente.");
                }

                string sql = @"UPDATE itemComanda SET quantidade = @quantidade, precoUnitario = @precoUnitario
                    WHERE itemComandaId = @itemComandaId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmd.Parameters.AddWithValue("@precoUnitario", item.PrecoUnitario);
                    cmd.Parameters.AddWithValue("@itemComandaId", item.ItemComandaId);
                    cmd.ExecuteNonQuery();
                }
                transacao.Commit();
            }
        }

        // Remove o item e devolve a quantidade para o estoque
        public void Delete(int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var transacao = conexao.BeginTransaction())
            {
                string sqlEstoque = @"UPDATE produto p
                    INNER JOIN itemComanda i ON i.FK_ProdutoId = p.produtoId
                    SET p.qtdEstoque = p.qtdEstoque + i.quantidade
                    WHERE i.itemComandaId = @itemComandaId";
                using (var cmd = new MySqlCommand(sqlEstoque, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@itemComandaId", id);
                    cmd.ExecuteNonQuery();
                }

                string sql = "DELETE FROM itemComanda WHERE itemComandaId = @itemComandaId";
                using (var cmd = new MySqlCommand(sql, conexao, transacao))
                {
                    cmd.Parameters.AddWithValue("@itemComandaId", id);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Nenhum item encontrado com esse ID.");
                }
                transacao.Commit();
            }
        }

        public List<ItemComanda> List()
        {
            return Listar(SelectBase + " ORDER BY i.FK_ComandaId", 0);
        }

        public List<ItemComanda> ListarPorComanda(int comandaId)
        {
            return Listar(SelectBase + " WHERE i.FK_ComandaId = @comandaId", comandaId);
        }

        public ItemComanda BuscarID(int id)
        {
            string sql = SelectBase + " WHERE i.itemComandaId = @itemComandaId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@itemComandaId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhum item encontrado com esse ID.");
                }
            }
        }

        private List<ItemComanda> Listar(string sql, int comandaId)
        {
            List<ItemComanda> itens = new List<ItemComanda>();
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@comandaId", comandaId);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        itens.Add(Mapear(dr));
                }
            }
            return itens;
        }

        private ItemComanda Mapear(MySqlDataReader dr)
        {
            ItemComanda itemComanda = new ItemComanda();
            itemComanda.ItemComandaId = Convert.ToInt32(dr["itemComandaId"]);
            itemComanda.Quantidade = Convert.ToInt32(dr["quantidade"]);
            itemComanda.PrecoUnitario = Convert.ToDouble(dr["precoUnitario"]);
            itemComanda.Comanda = new Comanda();
            itemComanda.Comanda.ComandaId = Convert.ToInt32(dr["FK_ComandaId"]);
            itemComanda.Produto = new Produto();
            itemComanda.Produto.ProdutoId = Convert.ToInt32(dr["FK_ProdutoId"]);
            itemComanda.Produto.Nome = dr["nomeProduto"].ToString();
            return itemComanda;
        }
    }
}
