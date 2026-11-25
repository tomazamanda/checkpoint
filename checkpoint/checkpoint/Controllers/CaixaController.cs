using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/caixa")]
    public class CaixaController : ControllerBase
    {
        private ICaixaDAO _caixaDao;
        private IComandaDAO _comandaDao;
        private IDao<Funcionario> _funcionarioDao;

        public CaixaController(ICaixaDAO caixaDao, IComandaDAO comandaDao, IDao<Funcionario> funcionarioDao)
        {
            _caixaDao = caixaDao;
            _comandaDao = comandaDao;
            _funcionarioDao = funcionarioDao;
        }

        // GET /api/caixa/aberto
        // Sem caixa aberto, o BuscarAberto devolve null e a API responde 204 (sem conteúdo).
        [HttpGet("aberto")]
        public Caixa Aberto()
        {
            return _caixaDao.BuscarAberto();
        }

        // POST /api/caixa/abrir
        [HttpPost("abrir")]
        public Caixa Abrir(AberturaCaixa dados)
        {
            if (_caixaDao.BuscarAberto() != null)
                throw new Exception("Já existe um caixa aberto.");

            Caixa caixa = new Caixa();
            caixa.Funcionario = _funcionarioDao.BuscarID(Usuario.FuncionarioId(User));
            caixa.ValorAbertura = dados.ValorAbertura;
            _caixaDao.Insert(caixa);
            return caixa;
        }

        // POST /api/caixa/fechar
        [HttpPost("fechar")]
        public Caixa Fechar()
        {
            Caixa caixa = _caixaDao.BuscarAberto();
            if (caixa == null)
                throw new Exception("Não há caixa aberto.");

            foreach (Comanda comanda in _comandaDao.ListarAbertas())
            {
                if (comanda.Caixa.CaixaId == caixa.CaixaId)
                    throw new Exception($"A comanda {comanda.ComandaId} ainda está aberta neste caixa.");
            }

            caixa.Fechar(_caixaDao.TotalComandasFechadas(caixa.CaixaId));
            _caixaDao.Update(caixa);
            return caixa;
        }
    }
}
