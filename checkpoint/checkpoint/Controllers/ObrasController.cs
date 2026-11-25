using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/obras")]
    public class ObrasController : ControllerBase
    {
        private IDao<Obra> _obraDao;

        public ObrasController(IDao<Obra> obraDao)
        {
            _obraDao = obraDao;
        }

        // GET /api/obras
        [HttpGet]
        public List<Obra> Listar()
        {
            return _obraDao.List();
        }
    }
}
