using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/funcionarios")]
    public class FuncionariosController : ControllerBase
    {
        private IDao<Funcionario> _funcionarioDao;

        public FuncionariosController(IDao<Funcionario> funcionarioDao)
        {
            _funcionarioDao = funcionarioDao;
        }

        // GET /api/funcionarios
        [HttpGet]
        public List<Funcionario> Listar()
        {
            return _funcionarioDao.List();
        }
    }
}
