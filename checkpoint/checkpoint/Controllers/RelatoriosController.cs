using checkpoint.DAO;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/relatorios")]
    public class RelatoriosController : ControllerBase
    {
        private RelatorioDAO _relatorioDao;

        public RelatoriosController(RelatorioDAO relatorioDao)
        {
            _relatorioDao = relatorioDao;
        }

        // GET /api/relatorios/faturamento
        [HttpGet("faturamento")]
        public List<LinhaRelatorio> Faturamento()
        {
            return _relatorioDao.FaturamentoPorFormaPagamento();
        }

        // GET /api/relatorios/sessoes
        [HttpGet("sessoes")]
        public List<LinhaRelatorio> Sessoes()
        {
            return _relatorioDao.SessoesPorTipoEstacao();
        }

        // GET /api/relatorios/produtos
        [HttpGet("produtos")]
        public List<LinhaRelatorio> Produtos()
        {
            return _relatorioDao.ProdutosMaisVendidos();
        }
    }
}
