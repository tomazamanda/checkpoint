import {
  api, moeda, dataHora, el, celula, carregarTabela, preencherSelect, selectComBusca, porNome,
  avisar, ligarFormulario, abrirDialogo, confirmar, numeroOuNulo, mascaraMoeda, valorDaMascara, minutosEmHoras
} from "./api.js";

selectComBusca(document.getElementById("comanda-cliente"));
selectComBusca(document.getElementById("sessao-cliente"));
selectComBusca(document.getElementById("sessao-estacao"));

const situacaoCaixa = document.getElementById("situacao-caixa");
const botaoAbrirCaixa = document.getElementById("botao-abrir-caixa");
const botaoFecharCaixa = document.getElementById("botao-fechar-caixa");
const botaoAbrirComanda = document.getElementById("botao-abrir-comanda");

let comandasAbertas = [];

// Caixa

async function carregarCaixa() {
  try {
    const caixa = await api("GET", "caixa/aberto");
    botaoAbrirCaixa.hidden = caixa !== null;
    botaoFecharCaixa.hidden = caixa === null;
    botaoAbrirComanda.disabled = caixa === null;

    if (caixa === null) {
      situacaoCaixa.textContent = "Nenhum caixa aberto. Abra o caixa para começar a atender.";
    } else {
      situacaoCaixa.textContent =
        `Caixa ${caixa.caixaId} · ${caixa.funcionario.nome} · aberto em ${dataHora(caixa.dataHoraAbertura)}`;
    }
  } catch (erro) {
    situacaoCaixa.textContent = "Erro: " + erro.message;
  }
}

botaoAbrirCaixa.addEventListener("click", () => abrirDialogo("dialogo-abrir-caixa"));

mascaraMoeda(document.getElementById("caixa-valor"));

ligarFormulario(document.getElementById("form-abrir-caixa"), async (dados) => {
  const caixa = await api("POST", "caixa/abrir", {
    valorAbertura: valorDaMascara(dados.get("valorAbertura"))
  });
  avisar(`Caixa ${caixa.caixaId} aberto.`);
  await carregarCaixa();
});

botaoFecharCaixa.addEventListener("click", async () => {
  // A API também barra (CaixaController), mas avisar aqui evita o pedido e mostra quais comandas faltam
  if (comandasAbertas.length > 0) {
    const numeros = comandasAbertas.map((c) => c.comandaId).join(", ");
    avisar(`Feche as comandas abertas antes de fechar o caixa (${comandasAbertas.length === 1 ? "comanda" : "comandas"} ${numeros}).`, "erro");
    return;
  }
  const confirmou = await confirmar(
    "Fechar o caixa agora? O valor de fechamento soma a abertura e as comandas fechadas neste caixa.",
    "Fechar caixa");
  if (!confirmou) return;
  try {
    const caixa = await api("POST", "caixa/fechar");
    avisar(`Caixa ${caixa.caixaId} fechado. Valor de fechamento: ${moeda(caixa.valorFechamento)}.`);
    await carregarCaixa();
  } catch (erro) {
    avisar(erro.message, "erro");
  }
});

// Comandas

async function carregarComandas() {
  comandasAbertas = await carregarTabela(
    document.getElementById("lista-comandas"), 4,
    () => api("GET", "comandas/abertas"),
    (comanda) => el("tr", {},
      celula(comanda.comandaId),
      celula(comanda.cliente ? comanda.cliente.nome : "Sem cadastro"),
      celula(dataHora(comanda.dataHoraAbertura)),
      el("td", {}, el("a", {
        href: `/comanda.html?id=${comanda.comandaId}`, class: "botao-secundario botao-pequeno",
        texto: "Lançar e fechar", "aria-label": `Lançar produtos e fechar a comanda ${comanda.comandaId}`
      }))),
    "Nenhuma comanda aberta. Clique em Abrir comanda para começar.");
}

botaoAbrirComanda.addEventListener("click", async () => {
  const clientes = await api("GET", "clientes");
  preencherSelect(document.getElementById("comanda-cliente"), clientes.sort(porNome),
    (c) => c.clienteId, (c) => c.nome, "Sem cadastro");
  abrirDialogo("dialogo-abrir-comanda");
});

ligarFormulario(document.getElementById("form-abrir-comanda"), async (dados) => {
  const comanda = await api("POST", "comandas", { clienteId: numeroOuNulo(dados.get("clienteId")) });
  avisar(`Comanda ${comanda.comandaId} aberta.`);
  await carregarComandas();
});

// Sessões

async function carregarSessoes() {
  await carregarTabela(
    document.getElementById("lista-sessoes"), 5,
    () => api("GET", "sessoes/abertas"),
    (sessao) => el("tr", {},
      celula(sessao.estacao.numero),
      celula(sessao.cliente.nome),
      celula(dataHora(sessao.dataHoraInicio)),
      celula(sessao.origem === "paga" ? `Comanda ${sessao.comanda.comandaId}` : "Crédito social"),
      el("td", {}, el("button", {
        type: "button", class: "botao-secundario botao-pequeno", texto: "Encerrar",
        "aria-label": `Encerrar sessão da estação ${sessao.estacao.numero}`,
        onclick: () => encerrarSessao(sessao)
      }))),
    "Nenhuma estação em uso agora.");
}

async function encerrarSessao(sessao) {
  const confirmou = await confirmar(
    `Encerrar a sessão de ${sessao.cliente.nome} na estação ${sessao.estacao.numero}? O tempo e o valor são calculados agora.`,
    "Encerrar sessão");
  if (!confirmou) return;
  try {
    const encerrada = await api("POST", `sessoes/${sessao.sessaoId}/encerrar`);
    avisar(`Sessão encerrada: ${encerrada.minutos} min, ${moeda(encerrada.valor)}.`);
    await Promise.all([carregarSessoes(), carregarEstacoes()]);
  } catch (erro) {
    avisar(erro.message, "erro");
  }
}

const campoComanda = document.getElementById("campo-comanda");
const selectComanda = document.getElementById("sessao-comanda");
const formSessao = document.getElementById("form-iniciar-sessao");

function mostrarCampoComanda() {
  const paga = formSessao.elements.origem.value === "paga";
  campoComanda.hidden = !paga;
  selectComanda.required = paga;
}

// Sessão paga só entra numa comanda do próprio cliente (regra do Sessao.Validar)
function atualizarComandas() {
  const clienteId = Number(document.getElementById("sessao-cliente").value);
  const doCliente = comandasAbertas.filter((c) => c.cliente && c.cliente.clienteId === clienteId);

  let primeiraOpcao = "Escolha a comanda";
  if (!clienteId) primeiraOpcao = "Escolha o cliente primeiro";
  else if (doCliente.length === 0) primeiraOpcao = "Este cliente não tem comanda aberta";

  preencherSelect(selectComanda, doCliente,
    (c) => c.comandaId, (c) => `Comanda ${c.comandaId}, aberta em ${dataHora(c.dataHoraAbertura)}`, primeiraOpcao);
  if (doCliente.length === 1) selectComanda.value = doCliente[0].comandaId;
}

const saldoSocial = document.getElementById("saldo-social");

// O crédito do projeto social aparece assim que o cliente é escolhido, antes de tentar iniciar a sessão
async function mostrarSaldo() {
  const clienteId = Number(document.getElementById("sessao-cliente").value);
  saldoSocial.hidden = !clienteId;
  if (!clienteId) return;
  saldoSocial.textContent = "Consultando o crédito...";
  try {
    const saldo = await api("GET", `clientes/${clienteId}/saldo-social`);
    if (Number(document.getElementById("sessao-cliente").value) !== clienteId) return;   // trocou de cliente no meio
    saldoSocial.textContent = saldo.minutos > 0
      ? `Crédito do projeto social: ${saldo.minutos} min (${minutosEmHoras(saldo.minutos)}).`
      : "Este cliente não tem crédito do projeto social.";
  } catch (erro) {
    saldoSocial.textContent = "Erro: " + erro.message;
  }
}

formSessao.addEventListener("change", (evento) => {
  if (evento.target.id === "sessao-cliente") {
    atualizarComandas();
    mostrarSaldo();
  }
  mostrarCampoComanda();
});

document.getElementById("botao-iniciar-sessao").addEventListener("click", async () => {
  const [clientes, estacoes] = await Promise.all([api("GET", "clientes"), api("GET", "estacoes/disponiveis")]);
  preencherSelect(document.getElementById("sessao-cliente"), clientes.sort(porNome),
    (c) => c.clienteId, (c) => c.nome, "Escolha o cliente");
  preencherSelect(document.getElementById("sessao-estacao"), estacoes,
    (e) => e.estacaoId, (e) => `${e.numero}: ${e.plataforma} (${moeda(e.precoHora)}/h)`, "Escolha a estação");
  atualizarComandas();
  mostrarSaldo();
  mostrarCampoComanda();
  abrirDialogo("dialogo-iniciar-sessao");
});

ligarFormulario(formSessao, async (dados) => {
  const origem = dados.get("origem");
  const sessao = await api("POST", "sessoes", {
    clienteId: Number(dados.get("clienteId")),
    estacaoId: Number(dados.get("estacaoId")),
    origem,
    comandaId: origem === "paga" ? numeroOuNulo(dados.get("comandaId")) : null
  });
  avisar(`Sessão iniciada na estação ${sessao.estacao.numero}.`);
  await Promise.all([carregarSessoes(), carregarEstacoes()]);
});

// Estações

async function carregarEstacoes() {
  const resumo = document.getElementById("resumo-estacoes");
  const estacoes = await carregarTabela(
    document.getElementById("lista-estacoes"), 4,
    () => api("GET", "estacoes"),
    (estacao) => el("tr", {},
      celula(estacao.numero),
      celula(estacao.tipo),
      celula(estacao.plataforma),
      celula(moeda(estacao.precoHora), "numero")),
    "Nenhuma estação cadastrada.", document.getElementById("paginacao-estacoes"));

  const livres = estacoes.filter((e) => e.status === "disponivel").length;
  const manutencao = estacoes.filter((e) => e.status === "manutencao").length;
  resumo.textContent = `${livres} de ${estacoes.length} livres, ${manutencao} em manutenção. Ver todas`;
}

carregarCaixa();
carregarComandas();
carregarSessoes();
carregarEstacoes();
