using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/estacoes")]
    public class EstacoesController : ControllerBase
    {
        private IEstacaoDAO _estacaoDao;

        public EstacoesController(IEstacaoDAO estacaoDao)
        {
            _estacaoDao = estacaoDao;
        }

        // GET /api/estacoes
        [HttpGet]
        public List<Estacao> Listar()
        {
            return _estacaoDao.List();
        }

        // GET /api/estacoes/disponiveis
        [HttpGet("disponiveis")]
        public List<Estacao> Disponiveis()
        {
            return _estacaoDao.ListarDisponiveis();
        }
    }
}
