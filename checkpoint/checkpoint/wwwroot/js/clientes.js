import {
  api, el, celula, carregarTabela, mostrarPagina, preencherSelect, selectComBusca, porNome, idade,
  minutosEmHoras, avisar, ligarFormulario, abrirDialogo, numeroOuNulo, mascaraCpf
} from "./api.js";

selectComBusca(document.getElementById("cliente-responsavel"));

mascaraCpf(document.getElementById("cliente-cpf"));

const corpo = document.getElementById("lista-clientes");
const busca = document.getElementById("busca");
const paginacao = document.getElementById("paginacao-clientes");
let clientes = [];

function montarLinha(cliente) {
  // O saldo é calculado no banco, um pedido por linha; só as 10 linhas da página aparecem
  const celulaSaldo = el("td", { texto: "…" });
  api("GET", `clientes/${cliente.clienteId}/saldo-social`)
    .then((saldo) => { celulaSaldo.textContent = `${saldo.minutos} min (${minutosEmHoras(saldo.minutos)})`; })
    .catch(() => { celulaSaldo.textContent = "indisponível"; });

  const responsavel = cliente.responsavel ? `${cliente.responsavel.nome} (${cliente.parentesco})` : "";
  return el("tr", {},
    celula(cliente.nome),
    celula(idade(cliente.dataNasc), "numero"),
    celula(responsavel),
    celula(cliente.telefone),
    celulaSaldo);
}

function filtrar() {
  const termo = busca.value.trim().toLowerCase();
  const numeros = termo.replace(/\D/g, "");
  const achados = clientes.filter((c) =>
    c.nome.toLowerCase().includes(termo) || (numeros !== "" && c.cpf.includes(numeros)));

  if (achados.length === 0) {
    paginacao.replaceChildren();
    corpo.replaceChildren(el("tr", {}, el("td", {
      colspan: 5, class: "linha-estado", texto: `Nenhum cliente encontrado para "${busca.value}".`
    })));
  } else {
    mostrarPagina(corpo, paginacao, achados, montarLinha);   // a busca sempre volta para a página 1
  }
}

async function carregar() {
  clientes = await carregarTabela(corpo, 5,
    async () => (await api("GET", "clientes")).sort(porNome),
    montarLinha,
    "Nenhum cliente cadastrado. Clique em Cadastrar cliente.",
    paginacao);
  if (busca.value) filtrar();
}

busca.addEventListener("input", filtrar);

// Cadastro

const form = document.getElementById("form-cliente");
const grupoResponsavel = document.getElementById("grupo-responsavel");
const campoNascimento = document.getElementById("cliente-nascimento");

// O responsável só aparece para menor de idade, igual à regra da classe Cliente
function mostrarResponsavel() {
  const menor = campoNascimento.value !== "" && idade(campoNascimento.value) < 18;
  grupoResponsavel.hidden = !menor;
  document.getElementById("cliente-responsavel").required = menor;
  document.getElementById("cliente-parentesco").required = menor;
}

campoNascimento.addEventListener("change", mostrarResponsavel);

document.getElementById("botao-novo-cliente").addEventListener("click", async () => {
  const responsaveis = await api("GET", "responsaveis");
  preencherSelect(document.getElementById("cliente-responsavel"), responsaveis.sort(porNome),
    (r) => r.responsavelId, (r) => r.nome, "Escolha o responsável");
  mostrarResponsavel();
  abrirDialogo("dialogo-cliente");
});

ligarFormulario(form, async (dados) => {
  const menor = !grupoResponsavel.hidden;
  const cliente = await api("POST", "clientes", {
    cpf: dados.get("cpf"),
    nome: dados.get("nome"),
    dataNasc: dados.get("dataNasc"),
    telefone: dados.get("telefone") || null,
    email: dados.get("email") || null,
    responsavelId: menor ? numeroOuNulo(dados.get("responsavelId")) : null,
    parentesco: menor ? dados.get("parentesco") : null
  });
  avisar(`${cliente.nome} cadastrado.`);
  await carregar();
});

carregar();
