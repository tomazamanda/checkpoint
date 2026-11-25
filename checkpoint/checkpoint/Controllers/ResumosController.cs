using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/resumos")]
    public class ResumosController : ControllerBase
    {
        private IAtividadeSocialDAO<Resumo> _resumoDao;
        private IClienteDAO _clienteDao;
        private IDao<Obra> _obraDao;
        private IDao<Funcionario> _funcionarioDao;
        private IWebHostEnvironment _ambiente;   // dá o caminho da pasta wwwroot

        public ResumosController(IAtividadeSocialDAO<Resumo> resumoDao, IClienteDAO clienteDao,
            IDao<Obra> obraDao, IDao<Funcionario> funcionarioDao,
            IWebHostEnvironment ambiente)
        {
            _resumoDao = resumoDao;
            _clienteDao = clienteDao;
            _obraDao = obraDao;
            _funcionarioDao = funcionarioDao;
            _ambiente = ambiente;
        }

        // GET /api/resumos  (todas; o front separa as que estão em andamento)
        [HttpGet]
        public List<Resumo> Listar()
        {
            return _resumoDao.List();
        }

        // GET /api/resumos/entregues  (aguardando avaliação)
        [HttpGet("entregues")]
        public List<Resumo> Entregues()
        {
            return _resumoDao.ListarEntregues();
        }

        // POST /api/resumos
        [HttpPost]
        public Resumo Iniciar(NovoResumo dados)
        {
            Cliente cliente = _clienteDao.BuscarID(dados.ClienteId);
            Obra obra = _obraDao.BuscarID(dados.ObraId);
            if (cliente.Idade() < obra.FaixaEtaria)
                throw new Exception($"Obra indicada para {obra.FaixaEtaria}+ anos. {cliente.Nome} tem {cliente.Idade()}.");
            AtividadeSocial.ConferirLimiteSemanal(cliente, _clienteDao.MinutosGanhosNaSemana(cliente.ClienteId, DateTime.Now));

            Resumo resumo = new Resumo();
            resumo.Cliente = cliente;
            resumo.Obra = obra;
            _resumoDao.Insert(resumo);
            return resumo;
        }

        // POST /api/resumos/5/entregar
        // O front manda um formulário com o arquivo no campo "foto" (não é JSON)
        [HttpPost("{id}/entregar")]
        public Resumo Entregar(int id, IFormFile foto)
        {
            Resumo resumo = _resumoDao.BuscarID(id);
            string caminho = Fotos.Salvar(foto, _ambiente.WebRootPath, "resumos", id);
            try
            {
                resumo.Entregar(caminho);   // confere se ainda está em andamento
                _resumoDao.Update(resumo);
            }
            catch
            {
                Fotos.Apagar(_ambiente.WebRootPath, caminho);   // entrega recusada: a foto não fica sobrando
                // Pesquisei para estudar: throw; sem nada depois joga de novo o mesmo erro que o catch pegou, para a mensagem continuar chegando na tela.
                throw;
            }
            return resumo;
        }

        // POST /api/resumos/5/avaliar
        [HttpPost("{id}/avaliar")]
        public Resumo Avaliar(int id, Avaliacao dados)
        {
            Usuario.ConferirAvaliador(User);
            Resumo resumo = _resumoDao.BuscarID(id);
            Funcionario avaliador = _funcionarioDao.BuscarID(Usuario.FuncionarioId(User));
            int ganhos = _clienteDao.MinutosGanhosNaSemana(resumo.Cliente.ClienteId, resumo.DataHoraEntrega.Value);
            resumo.Avaliar(avaliador, dados.Aprovado, dados.Observacao, ganhos);   // minutos vêm da obra, até o limite semanal
            _resumoDao.Update(resumo);
            return resumo;
        }
    }
}
