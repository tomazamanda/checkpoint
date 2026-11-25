import {
  api, dataHora, el, celula, carregarTabela, preencherSelect, selectComBusca, porNome,
  avisar, ligarFormulario, abrirDialogo, usuarioLogado, podeAvaliar
} from "./api.js";

selectComBusca(document.getElementById("atividade-cliente"));
selectComBusca(document.getElementById("atividade-obra"));
selectComBusca(document.getElementById("atividade-desafio"));

// Resumo e resolução têm os mesmos campos de atividade, herdados de AtividadeSocial, essas funções escondem o que muda entre os dois tipos.
function idDe(atividade) {
  return atividade.resumoId ?? atividade.resolucaoId;
}

function rotaDe(atividade) {
  return atividade.resumoId ? "resumos" : "resolucoes";
}

function descrever(atividade) {
  return atividade.obra
    ? `Resumo: ${atividade.obra.titulo}`
    : `Desafio: ${atividade.desafio.titulo}`;
}

function botaoAcao(texto, atividade, acao) {
  return el("td", {}, el("button", {
    type: "button", class: "botao-secundario botao-pequeno", texto,
    "aria-label": `${texto}: ${descrever(atividade)}, de ${atividade.cliente.nome}`,
    onclick: () => acao(atividade)
  }));
}

// Listas

async function carregarEntregues() {
  const avaliador = podeAvaliar(await usuarioLogado);
  await carregarTabela(document.getElementById("lista-entregues"), 5,
    async () => {
      const [resumos, resolucoes] = await Promise.all([
        api("GET", "resumos/entregues"), api("GET", "resolucoes/entregues")]);
      return [...resumos, ...resolucoes]
        .sort((a, b) => b.dataHoraEntrega.localeCompare(a.dataHoraEntrega));   // mais recente no topo: a entrega que acabou de ser feita aparece na 1ª página
    },
    (atividade) => el("tr", {},
      celula(descrever(atividade)),
      celula(atividade.cliente.nome),
      celula(dataHora(atividade.dataHoraEntrega)),
      linkFoto(atividade),
      avaliador ? botaoAcao("Avaliar", atividade, abrirAvaliacao) : celula("Só educador ou gerente", "dica")),
    "Nenhuma atividade esperando avaliação.",
    document.getElementById("paginacao-entregues"));
}

async function carregarAndamento() {
  await carregarTabela(document.getElementById("lista-andamento"), 4,
    async () => {
      const [resumos, resolucoes] = await Promise.all([api("GET", "resumos"), api("GET", "resolucoes")]);
      return [...resumos, ...resolucoes]
        .filter((a) => a.status === "em andamento")
        .sort((a, b) => a.dataHoraInicio.localeCompare(b.dataHoraInicio));
    },
    (atividade) => el("tr", {},
      celula(descrever(atividade)),
      celula(atividade.cliente.nome),
      celula(dataHora(atividade.dataHoraInicio)),
      botaoAcao("Registrar entrega", atividade, abrirEntrega)),
    "Nenhuma atividade em andamento.",
    document.getElementById("paginacao-andamento"));
}

// Iniciar

const formIniciar = document.getElementById("form-iniciar");
const campoObra = document.getElementById("campo-obra");
const campoDesafio = document.getElementById("campo-desafio");

function mostrarCampoDoTipo() {
  const resumo = formIniciar.elements.tipo.value === "resumos";
  campoObra.hidden = !resumo;
  campoDesafio.hidden = resumo;
  document.getElementById("atividade-obra").required = resumo;
  document.getElementById("atividade-desafio").required = !resumo;
}

formIniciar.addEventListener("change", mostrarCampoDoTipo);

document.getElementById("botao-iniciar").addEventListener("click", async () => {
  const [clientes, obras, desafios] = await Promise.all([
    api("GET", "clientes"), api("GET", "obras"), api("GET", "desafios")]);
  preencherSelect(document.getElementById("atividade-cliente"), clientes.sort(porNome),
    (c) => c.clienteId, (c) => c.nome, "Escolha o cliente");
  preencherSelect(document.getElementById("atividade-obra"),
    obras.sort((a, b) => a.titulo.localeCompare(b.titulo, "pt-BR")),
    (o) => o.obraId, (o) => `${o.titulo} (${o.tipo}, a partir de ${o.faixaEtaria} anos, vale ${o.minutosRecompensa} min)`,
    "Escolha a obra");
  preencherSelect(document.getElementById("atividade-desafio"),
    desafios.sort((a, b) => a.titulo.localeCompare(b.titulo, "pt-BR")),
    (d) => d.desafioId, (d) => `${d.titulo} (${d.area}, ${d.nivel}, vale ${d.minutosRecompensa} min)`,
    "Escolha o desafio");
  mostrarCampoDoTipo();
  abrirDialogo("dialogo-iniciar");
});

ligarFormulario(formIniciar, async (dados) => {
  const tipo = dados.get("tipo");
  const corpo = tipo === "resumos"
    ? { clienteId: Number(dados.get("clienteId")), obraId: Number(dados.get("obraId")) }
    : { clienteId: Number(dados.get("clienteId")), desafioId: Number(dados.get("desafioId")) };
  const atividade = await api("POST", tipo, corpo);
  avisar(`${descrever(atividade)} iniciado para ${atividade.cliente.nome}.`);
  await carregarAndamento();
});

// A foto fica em wwwroot/fotos; o link abre numa aba nova (só com login)
function linkFoto(atividade) {
  if (!atividade.fotoUrl) return celula("sem foto");
  return el("td", {}, el("a", { href: "/" + atividade.fotoUrl, target: "_blank", rel: "noopener", texto: "Ver foto" }));
}

// Entregar

let atividadeAtual = null;

function abrirEntrega(atividade) {
  atividadeAtual = atividade;
  document.getElementById("entregar-descricao").textContent = `${descrever(atividade)}, de ${atividade.cliente.nome}`;
  abrirDialogo("dialogo-entregar");
}

ligarFormulario(document.getElementById("form-entregar"), async (dados) => {
  // a API também confere; aqui evita mandar um arquivo grande à toa
  if (dados.get("foto").size > 5 * 1024 * 1024) throw new Error("A foto pode ter até 5 MB.");
  await api("POST", `${rotaDe(atividadeAtual)}/${idDe(atividadeAtual)}/entregar`, dados);
  avisar(`Entrega registrada para ${atividadeAtual.cliente.nome}. A atividade agora espera avaliação.`);
  await Promise.all([carregarAndamento(), carregarEntregues()]);
});

// Avaliar

// A avaliação fica no nome de quem está logado (só educador social ou gerente)
function abrirAvaliacao(atividade) {
  atividadeAtual = atividade;
  document.getElementById("avaliar-descricao").textContent = `${descrever(atividade)}, de ${atividade.cliente.nome}`;
  document.getElementById("avaliar-foto").href = "/" + atividade.fotoUrl;
  abrirDialogo("dialogo-avaliar");
}

ligarFormulario(document.getElementById("form-avaliar"), async (dados) => {
  const avaliada = await api("POST", `${rotaDe(atividadeAtual)}/${idDe(atividadeAtual)}/avaliar`, {
    aprovado: dados.get("resultado") === "aprovado",
    observacao: dados.get("observacao") || null
  });
  const valeria = (avaliada.obra ?? avaliada.desafio).minutosRecompensa;
  const cortado = avaliada.minutosConcedidos < valeria ? ` (valeria ${valeria}, mas bateu o limite semanal)` : "";
  const resultado = avaliada.status === "aprovado"
    ? `aprovado, ${avaliada.minutosConcedidos} min de crédito para ${avaliada.cliente.nome}${cortado}`
    : "recusado";
  avisar(`${descrever(avaliada)}: ${resultado}.`);
  await carregarEntregues();
});

carregarEntregues();
carregarAndamento();
