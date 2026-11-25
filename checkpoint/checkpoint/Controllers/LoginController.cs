using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace checkpoint.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private IFuncionarioDAO _funcionarioDao;

        public LoginController(IFuncionarioDAO funcionarioDao)
        {
            _funcionarioDao = funcionarioDao;
        }

        // POST /api/login
         /* Confere e-mail e senha e grava um cookie de sessão no navegador.
         Os pedidos seguintes levam o cookie, e a API sabe quem está logado.
         Pesquisei para estudar: async/await e Task. O SignInAsync é assíncrono, então o método espera ele terminar com await.
         [AllowAnonymous] deixa usar este endereço sem estar logado. */
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Entrar(Login dados)
        {
            string email = "";
            if (dados.Email != null)
                email = dados.Email.Trim();
            Funcionario funcionario = _funcionarioDao.BuscarPorEmail(email);

            // A mesma mensagem para e-mail e senha errados: não revela quais e-mails existem
            if (funcionario == null || !funcionario.ConferirSenha(dados.Senha))
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });

            /* Pesquisei para estudar: Claim. Cada Claim é um dado de quem entrou (ID, nome e cargo), e o SignInAsync guarda esses dados no cookie. */
            List<Claim> dadosDoLogin = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, funcionario.FuncionarioId.ToString()),
                new Claim(ClaimTypes.Name, funcionario.Nome),
                new Claim(ClaimTypes.Role, funcionario.Cargo)
            };
            ClaimsIdentity identidade = new ClaimsIdentity(dadosDoLogin, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

            return Ok(new { funcionario.FuncionarioId, funcionario.Nome, funcionario.Cargo });
        }

        // POST /api/login/sair
        [HttpPost("sair")]
        [AllowAnonymous]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        // GET /api/login/eu  (quem está logado; o topo das telas mostra o nome)
        [HttpGet("eu")]
        public object Eu()
        {
            return new
            {
                funcionarioId = Usuario.FuncionarioId(User),
                nome = User.FindFirstValue(ClaimTypes.Name),
                cargo = User.FindFirstValue(ClaimTypes.Role)
            };
        }
    }
}
