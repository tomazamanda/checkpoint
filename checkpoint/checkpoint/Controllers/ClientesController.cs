using checkpoint.DAO;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private IClienteDAO _clienteDao;
        private IDao<Responsavel> _responsavelDao;

        public ClientesController(IClienteDAO clienteDao, IDao<Responsavel> responsavelDao)
        {
            _clienteDao = clienteDao;
            _responsavelDao = responsavelDao;
        }

        // GET /api/clientes
        [HttpGet]
        public List<Cliente> Listar()
        {
            return _clienteDao.List();
        }

        // GET /api/clientes/5
        [HttpGet("{id}")]
        public Cliente Buscar(int id)
        {
            return _clienteDao.BuscarID(id);
        }

        // POST /api/clientes
        [HttpPost]
        public Cliente Cadastrar(NovoCliente dados)
        {
            Cliente cliente = new Cliente();
            cliente.Cpf = dados.Cpf;

            // CPF já cadastrado como responsável ou funcionário: reaproveita a pessoa
            if (!PessoaDAO.PreencherPorCpf(cliente, cliente.Cpf))
            {
                if (string.IsNullOrWhiteSpace(dados.Nome))
                    throw new Exception("Informe o nome.");
                if (dados.DataNasc == null)
                    throw new Exception("Informe a data de nascimento.");

                cliente.Nome = dados.Nome.Trim();
                cliente.DataNasc = dados.DataNasc.Value;
                cliente.Telefone = dados.Telefone;
                cliente.Email = dados.Email;
            }
            cliente.DataCadastro = DateOnly.FromDateTime(DateTime.Today);

            if (cliente.EhMenorDeIdade())
            {
                if (dados.ResponsavelId == null)
                    throw new Exception("Cliente menor de idade precisa de um responsável.");
                if (string.IsNullOrWhiteSpace(dados.Parentesco))
                    throw new Exception("Informe o parentesco do responsável.");

                cliente.Responsavel = _responsavelDao.BuscarID(dados.ResponsavelId.Value);
                cliente.Parentesco = dados.Parentesco.Trim();
                cliente.DataAutorizacao = cliente.DataCadastro;
            }

            _clienteDao.Insert(cliente);
            return cliente;
        }

        // GET /api/clientes/5/saldo-social
        [HttpGet("{id}/saldo-social")]
        public object SaldoSocial(int id)
        {
            Cliente cliente = _clienteDao.BuscarID(id);
            int minutos = _clienteDao.SaldoMinutosSociais(cliente.ClienteId);
            return new { cliente = cliente.Nome, minutos };
        }
    }
}
