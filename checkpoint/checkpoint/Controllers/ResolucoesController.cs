using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/resolucoes")]
    public class ResolucoesController : ControllerBase
    {
        private IAtividadeSocialDAO<Resolucao> _resolucaoDao;
        private IClienteDAO _clienteDao;
        private IDao<Desafio> _desafioDao;
        private IDao<Funcionario> _funcionarioDao;
        private IWebHostEnvironment _ambiente;   // dá o caminho da pasta wwwroot

        public ResolucoesController(IAtividadeSocialDAO<Resolucao> resolucaoDao, IClienteDAO clienteDao,
            IDao<Desafio> desafioDao, IDao<Funcionario> funcionarioDao,
            IWebHostEnvironment ambiente)
        {
            _resolucaoDao = resolucaoDao;
            _clienteDao = clienteDao;
            _desafioDao = desafioDao;
            _funcionarioDao = funcionarioDao;
            _ambiente = ambiente;
        }

        // GET /api/resolucoes  (todas; o front separa as que estão em andamento)
        [HttpGet]
        public List<Resolucao> Listar()
        {
            return _resolucaoDao.List();
        }

        // GET /api/resolucoes/entregues  (aguardando avaliação)
        [HttpGet("entregues")]
        public List<Resolucao> Entregues()
        {
            return _resolucaoDao.ListarEntregues();
        }

        // POST /api/resolucoes
        [HttpPost]
        public Resolucao Iniciar(NovaResolucao dados)
        {
            Resolucao resolucao = new Resolucao();
            resolucao.Cliente = _clienteDao.BuscarID(dados.ClienteId);
            resolucao.Desafio = _desafioDao.BuscarID(dados.DesafioId);
            AtividadeSocial.ConferirLimiteSemanal(resolucao.Cliente,
                _clienteDao.MinutosGanhosNaSemana(resolucao.Cliente.ClienteId, DateTime.Now));
            _resolucaoDao.Insert(resolucao);
            return resolucao;
        }

        // POST /api/resolucoes/5/entregar
        // O front manda um formulário com o arquivo no campo "foto" (não é JSON)
        [HttpPost("{id}/entregar")]
        public Resolucao Entregar(int id, IFormFile foto)
        {
            Resolucao resolucao = _resolucaoDao.BuscarID(id);
            string caminho = Fotos.Salvar(foto, _ambiente.WebRootPath, "resolucoes", id);
            try
            {
                resolucao.Entregar(caminho);   // confere se ainda está em andamento
                _resolucaoDao.Update(resolucao);
            }
            catch
            {
                Fotos.Apagar(_ambiente.WebRootPath, caminho);   // entrega recusada: a foto não fica sobrando
                // Pesquisei para estudar: throw; sem nada depois joga de novo o mesmo erro que o catch pegou, para a mensagem continuar chegando na tela.
                throw;
            }
            return resolucao;
        }

        // POST /api/resolucoes/5/avaliar
        [HttpPost("{id}/avaliar")]
        public Resolucao Avaliar(int id, Avaliacao dados)
        {
            Usuario.ConferirAvaliador(User);
            Resolucao resolucao = _resolucaoDao.BuscarID(id);
            Funcionario avaliador = _funcionarioDao.BuscarID(Usuario.FuncionarioId(User));
            int ganhos = _clienteDao.MinutosGanhosNaSemana(resolucao.Cliente.ClienteId, resolucao.DataHoraEntrega.Value);
            resolucao.Avaliar(avaliador, dados.Aprovado, dados.Observacao, ganhos);   // minutos vêm do desafio, até o limite semanal
            _resolucaoDao.Update(resolucao);
            return resolucao;
        }
    }
}
