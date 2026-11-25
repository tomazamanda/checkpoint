using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/responsaveis")]
    public class ResponsaveisController : ControllerBase
    {
        private IDao<Responsavel> _responsavelDao;

        public ResponsaveisController(IDao<Responsavel> responsavelDao)
        {
            _responsavelDao = responsavelDao;
        }

        // GET /api/responsaveis
        [HttpGet]
        public List<Responsavel> Listar()
        {
            return _responsavelDao.List();
        }
    }
}
