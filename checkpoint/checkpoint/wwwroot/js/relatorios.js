import { api, moeda, el, celula, carregarTabela, plural, emEstoque } from "./api.js";

const nomesPagamento = { dinheiro: "Dinheiro", pix: "Pix", debito: "Débito", credito: "Crédito" };

function linhaRelatorio(nomes) {
  return (linha) => el("tr", {},
    celula(nomes?.[linha.descricao] ?? linha.descricao),
    celula(linha.quantidade, "numero"),
    celula(moeda(linha.total), "numero"));
}

// Escreve a conclusão acima da tabela, a partir da linha com o maior valor no critério
function concluir(id, lista, frase, criterio = (linha) => linha.total) {
  if (lista.length === 0) {
    document.getElementById(id).textContent = "";
    return;
  }
  const maior = lista.reduce((a, b) => (criterio(b) > criterio(a) ? b : a));
  document.getElementById(id).textContent = frase(maior, lista);
}

async function carregarFaturamento() {
  const lista = await carregarTabela(document.getElementById("lista-faturamento"), 3,
    () => api("GET", "relatorios/faturamento"), linhaRelatorio(nomesPagamento), "Nenhuma comanda fechada ainda.");
  concluir("conclusao-faturamento", lista, (maior, todas) => {
    const total = todas.reduce((soma, l) => soma + l.total, 0);
    const parte = Math.round((maior.total / total) * 100);
    return `${nomesPagamento[maior.descricao]} lidera: ${moeda(maior.total)} em ${plural(maior.quantidade, "comanda", "comandas")} (${parte}% do faturamento).`;
  });
}

async function carregarSessoes() {
  const lista = await carregarTabela(document.getElementById("lista-sessoes"), 3,
    () => api("GET", "relatorios/sessoes"), linhaRelatorio(), "Nenhuma sessão encerrada ainda.");
  concluir("conclusao-sessoes", lista,
    (maior) => `Estações do tipo ${maior.descricao} somam ${plural(maior.quantidade, "sessão", "sessões")} e ${moeda(maior.total)}.`);
}

async function carregarProdutos() {
  const lista = await carregarTabela(document.getElementById("lista-produtos"), 3,
    () => api("GET", "relatorios/produtos"), linhaRelatorio(), "Nenhum produto vendido ainda.");
  concluir("conclusao-produtos", lista,
    (maior) => `${maior.descricao} é o mais vendido: ${plural(maior.quantidade, "unidade", "unidades")}, ${moeda(maior.total)}.`,
    (linha) => linha.quantidade);
}

async function carregarEstoque() {
  const lista = await carregarTabela(document.getElementById("lista-estoque"), 4,
    () => api("GET", "produtos/estoque-baixo"),
    (produto) => el("tr", {},
      celula(produto.nome),
      celula(produto.tipo),
      el("td", { class: "numero texto-destaque", texto: emEstoque(produto) }),
      celula(produto.qtdMinima, "numero")),
    "Nenhum produto abaixo do mínimo.");
  concluir("conclusao-estoque", lista,
    (_, todos) => `${plural(todos.length, "produto", "produtos")} no mínimo ou abaixo dele.`);
}

carregarEstoque();
carregarFaturamento();
carregarSessoes();
carregarProdutos();

// Pesquisei para estudar: window.print() abre a mesma janela do Ctrl+P.
document.getElementById("botao-imprimir").addEventListener("click", () => window.print());
