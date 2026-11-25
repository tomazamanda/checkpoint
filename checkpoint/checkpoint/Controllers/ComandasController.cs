using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/comandas")]
    public class ComandasController : ControllerBase
    {
        private IComandaDAO _comandaDao;
        private ICaixaDAO _caixaDao;
        private IClienteDAO _clienteDao;
        private IProdutoDAO _produtoDao;
        private IItemComandaDAO _itemComandaDao;

        public ComandasController(IComandaDAO comandaDao, ICaixaDAO caixaDao, IClienteDAO clienteDao,
            IProdutoDAO produtoDao, IItemComandaDAO itemComandaDao)
        {
            _comandaDao = comandaDao;
            _caixaDao = caixaDao;
            _clienteDao = clienteDao;
            _produtoDao = produtoDao;
            _itemComandaDao = itemComandaDao;
        }

        // GET /api/comandas/abertas
        [HttpGet("abertas")]
        public List<Comanda> Abertas()
        {
            return _comandaDao.ListarAbertas();
        }

        // GET /api/comandas/5  (com os itens e as sessões)
        [HttpGet("{id}")]
        public Comanda Buscar(int id)
        {
            return _comandaDao.BuscarID(id);
        }

        // POST /api/comandas
        [HttpPost]
        public Comanda Abrir(NovaComanda dados)
        {
            Caixa caixa = _caixaDao.BuscarAberto();
            if (caixa == null)
                throw new Exception("Nenhum caixa aberto. Abra o caixa primeiro.");

            Comanda comanda = new Comanda();
            comanda.Caixa = caixa;
            if (dados.ClienteId != null)
                comanda.Cliente = _clienteDao.BuscarID(dados.ClienteId.Value);

            _comandaDao.Insert(comanda);
            return comanda;
        }

        // POST /api/comandas/5/itens
        [HttpPost("{id}/itens")]
        public ItemComanda AdicionarItem(int id, NovoItem dados)
        {
            Comanda comanda = _comandaDao.BuscarID(id);
            if (comanda.Status != "aberta")
                throw new Exception("Esta comanda não está aberta.");

            Produto produto = _produtoDao.BuscarID(dados.ProdutoId);
            if (produto.Tipo != "venda")
                throw new Exception("Material interno não é vendido.");

            ItemComanda item = new ItemComanda();
            item.Comanda = comanda;
            item.Produto = produto;
            item.PrecoUnitario = produto.PrecoVenda.Value;
            item.Quantidade = dados.Quantidade;

            _itemComandaDao.Insert(item);
            return item;
        }

        // POST /api/comandas/5/fechar
        [HttpPost("{id}/fechar")]
        public Comanda Fechar(int id, FechamentoComanda dados)
        {
            if (string.IsNullOrWhiteSpace(dados.FormaPagamento))
                throw new Exception("Escolha a forma de pagamento.");

            Comanda comanda = _comandaDao.BuscarID(id);
            comanda.Fechar(dados.FormaPagamento);
            _comandaDao.Update(comanda);
            return comanda;
        }

        // POST /api/comandas/5/cancelar
        [HttpPost("{id}/cancelar")]
        public Comanda Cancelar(int id)
        {
            Comanda comanda = _comandaDao.BuscarID(id);
            comanda.Cancelar();   // confere as regras antes de mexer no banco

            // o Delete do item devolve a quantidade ao estoque
            foreach (ItemComanda item in comanda.Itens)
                _itemComandaDao.Delete(item.ItemComandaId);
            comanda.Itens.Clear();

            _comandaDao.Update(comanda);
            return comanda;
        }
    }
}
