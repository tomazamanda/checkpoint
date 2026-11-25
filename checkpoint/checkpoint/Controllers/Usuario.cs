using System.Security.Claims;

namespace checkpoint.Controllers
{
    // Nos controllers, Usuario.FuncionarioId(User) devolve o ID de quem está logado.
    // O ID foi gravado no cookie pelo LoginController.
    public static class Usuario
    {
        public static int FuncionarioId(ClaimsPrincipal usuario)
        {
            return int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Só educador social e gerente avaliam resumos e desafios.
        public static void ConferirAvaliador(ClaimsPrincipal usuario)
        {
            string cargo = usuario.FindFirstValue(ClaimTypes.Role);
            if (cargo != "educador social" && cargo != "gerente")
                throw new Exception("Só o educador social ou o gerente pode avaliar.");
        }
    }
}
