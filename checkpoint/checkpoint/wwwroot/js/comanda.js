import {
  api, moeda, dataHora, el, celula, preencherSelect, selectComBusca, avisar, ligarFormulario, abrirDialogo,
  mascaraQuantidade
} from "./api.js";

mascaraQuantidade(document.getElementById("item-quantidade"));

selectComBusca(document.getElementById("item-produto"));

const id = new URLSearchParams(location.search).get("id");
const titulo = document.getElementById("titulo-comanda");
const dados = document.getElementById("dados-comanda");
const formItem = document.getElementById("form-item");
const botaoFechar = document.getElementById("botao-fechar-comanda");
const botaoCancelar = document.getElementById("botao-cancelar-comanda");
const corpoItens = document.getElementById("lista-itens");
const corpoSessoes = document.getElementById("lista-sessoes");

const formasPagamento = { dinheiro: "em dinheiro", pix: "no Pix", debito: "no débito", credito: "no crédito" };

function linhaVazia(colunas, texto) {
  return el("tr", {}, el("td", { colspan: colunas, class: "linha-estado", texto }));
}

function calcularTotal(comanda) {
  const itens = comanda.itens.reduce((soma, item) => soma + item.precoUnitario * item.quantidade, 0);
  const sessoes = comanda.sessoes.reduce((soma, sessao) => soma + (sessao.valor ?? 0), 0);
  return itens + sessoes;
}

function mostrar(comanda) {
  document.title = `Comanda ${comanda.comandaId} · Checkpoint`;
  titulo.textContent = `Comanda ${comanda.comandaId}`;

  const cliente = comanda.cliente ? comanda.cliente.nome : "Sem cadastro";
  let situacao = `${cliente} · aberta em ${dataHora(comanda.dataHoraAbertura)}`;
  if (comanda.status === "fechada") {
    situacao += ` · fechada em ${dataHora(comanda.dataHoraFechamento)}, pago ${formasPagamento[comanda.formaPagamento]}`;
  } else if (comanda.status === "cancelada") {
    situacao += ` · cancelada em ${dataHora(comanda.dataHoraFechamento)}`;
  }
  dados.textContent = situacao;

  const aberta = comanda.status === "aberta";
  formItem.hidden = !aberta;
  const vazia = comanda.itens.length === 0 && comanda.sessoes.length === 0;
  botaoFechar.hidden = !aberta || vazia;   // comanda vazia não fecha, cancela
  botaoCancelar.hidden = !aberta || comanda.sessoes.length > 0;   // com sessão, só fechando
  document.getElementById("nota-sessoes").hidden = !comanda.sessoes.some((s) => !s.dataHoraFim);

  corpoItens.replaceChildren(...(comanda.itens.length === 0
    ? [linhaVazia(4, aberta ? "Nenhum produto ainda. Escolha um abaixo." : "Nenhum produto.")]
    : comanda.itens.map((item) => el("tr", {},
        celula(item.produto.nome),
        celula(item.quantidade, "numero"),
        celula(moeda(item.precoUnitario), "numero"),
        celula(moeda(item.precoUnitario * item.quantidade), "numero")))));

  corpoSessoes.replaceChildren(...(comanda.sessoes.length === 0
    ? [linhaVazia(4, "Nenhuma sessão nesta comanda.")]
    : comanda.sessoes.map((sessao) => el("tr", {},
        celula(sessao.estacao.numero),
        celula(dataHora(sessao.dataHoraInicio)),
        celula(sessao.dataHoraFim ? sessao.minutos : "em andamento", "numero"),
        celula(sessao.dataHoraFim ? moeda(sessao.valor) : "", "numero")))));

  const total = comanda.status === "fechada" ? comanda.valorTotal : calcularTotal(comanda);
  document.getElementById("total-comanda").textContent = moeda(total);
  document.getElementById("total-fechamento").textContent = `Total a pagar: ${moeda(total)}`;
}

async function carregar() {
  if (!id) {
    titulo.textContent = "Comanda não informada";
    dados.textContent = "Abra uma comanda pela lista do balcão.";
    return;
  }
  try {
    mostrar(await api("GET", `comandas/${id}`));
  } catch (erro) {
    dados.textContent = "Erro: " + erro.message;
  }
}

async function carregarProdutos() {
  const produtos = await api("GET", "produtos");
  const aVenda = produtos
    .filter((p) => p.tipo === "venda")
    .sort((a, b) => a.nome.localeCompare(b.nome, "pt-BR"));
  preencherSelect(document.getElementById("item-produto"), aVenda,
    (p) => p.produtoId, (p) => `${p.nome} (${moeda(p.precoVenda)}, ${p.qtdEstoque} em estoque)`, "Escolha o produto");
}

ligarFormulario(formItem, async (campos) => {
  const item = await api("POST", `comandas/${id}/itens`, {
    produtoId: Number(campos.get("produtoId")),
    quantidade: Number(campos.get("quantidade"))
  });
  avisar(`${item.quantidade}x ${item.produto.nome} adicionado.`);
  await Promise.all([carregar(), carregarProdutos()]);
});

botaoFechar.addEventListener("click", () => abrirDialogo("dialogo-fechar"));

ligarFormulario(document.getElementById("form-fechar"), async (campos) => {
  const comanda = await api("POST", `comandas/${id}/fechar`, { formaPagamento: campos.get("formaPagamento") });
  avisar(`Comanda ${comanda.comandaId} fechada. Total: ${moeda(comanda.valorTotal)}.`);
  await carregar();
});

botaoCancelar.addEventListener("click", () => abrirDialogo("dialogo-cancelar"));

ligarFormulario(document.getElementById("form-cancelar"), async () => {
  const comanda = await api("POST", `comandas/${id}/cancelar`);
  avisar(`Comanda ${comanda.comandaId} cancelada.`);
  await carregar();
});

carregar();
carregarProdutos();
