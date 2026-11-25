import { api, ligarFormulario } from "./api.js";

// Depois de entrar, volta para a tela que a pessoa tentou abrir (ReturnUrl).
// Só aceita caminho do próprio site: "//outro-site.com" levaria para fora.
function destino() {
  const volta = new URLSearchParams(location.search).get("ReturnUrl");
  return volta && volta.startsWith("/") && !volta.startsWith("//") && !volta.startsWith("/\\") ? volta : "/";
}

// Quem já está logado não precisa ver o login
api("GET", "login/eu").then(() => location.replace(destino())).catch(() => {});

ligarFormulario(document.getElementById("form-login"), async (campos) => {
  await api("POST", "login", { email: campos.get("email"), senha: campos.get("senha") });
  location.replace(destino());
});
