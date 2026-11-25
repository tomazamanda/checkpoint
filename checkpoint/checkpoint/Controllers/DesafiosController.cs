using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/desafios")]
    public class DesafiosController : ControllerBase
    {
        private IDao<Desafio> _desafioDao;

        public DesafiosController(IDao<Desafio> desafioDao)
        {
            _desafioDao = desafioDao;
        }

        // GET /api/desafios
        [HttpGet]
        public List<Desafio> Listar()
        {
            return _desafioDao.List();
        }
    }
}
