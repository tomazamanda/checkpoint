using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutosController : ControllerBase
    {
        private IProdutoDAO _produtoDao;

        public ProdutosController(IProdutoDAO produtoDao)
        {
            _produtoDao = produtoDao;
        }

        // GET /api/produtos
        [HttpGet]
        public List<Produto> Listar()
        {
            return _produtoDao.List();
        }

        // GET /api/produtos/estoque-baixo
        [HttpGet("estoque-baixo")]
        public List<Produto> EstoqueBaixo()
        {
            return _produtoDao.EstoqueBaixo();
        }

        // POST /api/produtos
        [HttpPost]
        public Produto Cadastrar(NovoProduto dados)
        {
            Produto produto = new Produto();
            if (dados.Nome != null)
                produto.Nome = dados.Nome.Trim();
            produto.Tipo = dados.Tipo;               // a propriedade valida "venda" / "material interno"
            if (dados.Unidade != null)
                produto.Unidade = dados.Unidade.Trim();
            produto.PrecoVenda = dados.PrecoVenda;
            produto.QtdEstoque = dados.QtdEstoque;   // a propriedade não aceita negativo
            produto.QtdMinima = dados.QtdMinima;
            produto.Validar();
            _produtoDao.Insert(produto);
            return produto;
        }

        // POST /api/produtos/5/repor
        [HttpPost("{id}/repor")]
        public Produto Repor(int id, Reposicao dados)
        {
            if (dados.Quantidade <= 0 || dados.Quantidade > 9999)
                throw new Exception("A quantidade reposta deve ser de 1 a 9999.");
            _produtoDao.ReporEstoque(id, dados.Quantidade);
            return _produtoDao.BuscarID(id);
        }
    }
}
