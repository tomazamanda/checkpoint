using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/sessoes")]
    public class SessoesController : ControllerBase
    {
        private ISessaoDAO _sessaoDao;
        private IClienteDAO _clienteDao;
        private IEstacaoDAO _estacaoDao;
        private IComandaDAO _comandaDao;

        public SessoesController(ISessaoDAO sessaoDao, IClienteDAO clienteDao,
            IEstacaoDAO estacaoDao, IComandaDAO comandaDao)
        {
            _sessaoDao = sessaoDao;
            _clienteDao = clienteDao;
            _estacaoDao = estacaoDao;
            _comandaDao = comandaDao;
        }

        // GET /api/sessoes/abertas
        [HttpGet("abertas")]
        public List<Sessao> Abertas()
        {
            return _sessaoDao.ListarAbertas();
        }

        // POST /api/sessoes
        [HttpPost]
        public Sessao Iniciar(NovaSessao dados)
        {
            Sessao sessao = new Sessao();
            sessao.Cliente = _clienteDao.BuscarID(dados.ClienteId);
            sessao.Estacao = _estacaoDao.BuscarID(dados.EstacaoId);
            sessao.Origem = dados.Origem;

            // A sessão não registra qual jogo será jogado, então vale o jogo de maior classificação da estação
            int classificacao = _estacaoDao.ClassificacaoMaxima(sessao.Estacao.EstacaoId);
            if (sessao.Cliente.Idade() < classificacao)
                throw new Exception($"A estação {sessao.Estacao.Numero} tem jogo para {classificacao}+ anos " +
                    $"e {sessao.Cliente.Nome} tem {sessao.Cliente.Idade()}. Escolha outra estação.");

            if (sessao.Origem == "credito social")
            {
                int saldo = _clienteDao.SaldoMinutosSociais(sessao.Cliente.ClienteId);
                if (saldo <= 0)
                    throw new Exception("Cliente sem saldo de horas sociais.");
            }
            if (dados.ComandaId != null)
                sessao.Comanda = _comandaDao.BuscarID(dados.ComandaId.Value);

            _sessaoDao.Insert(sessao);   // o Insert chama sessao.Validar()
            return sessao;
        }

        // POST /api/sessoes/5/encerrar
        [HttpPost("{id}/encerrar")]
        public Sessao Encerrar(int id)
        {
            Sessao sessao = _sessaoDao.BuscarID(id);
            sessao.Encerrar();
            _sessaoDao.Update(sessao);
            return sessao;
        }
    }
}
