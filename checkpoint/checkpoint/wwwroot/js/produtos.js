import {
  api, moeda, el, celula, carregarTabela, mostrarPagina, emEstoque, avisar,
  ligarFormulario, abrirDialogo, mascaraMoeda, valorDaMascara, mascaraQuantidade
} from "./api.js";

for (const id of ["produto-estoque", "produto-minimo", "repor-quantidade"])
  mascaraQuantidade(document.getElementById(id));

const corpo = document.getElementById("lista-produtos");
const busca = document.getElementById("busca");
const paginacao = document.getElementById("paginacao-produtos");
let produtos = [];

// Lista

function montarLinha(produto) {
  return el("tr", {},
    celula(produto.nome),
    celula(produto.tipo),
    celula(produto.precoVenda == null ? "uso interno" : moeda(produto.precoVenda), "numero"),
    celula(emEstoque(produto), "numero"),
    celula(produto.qtdMinima, "numero"),
    el("td", {}, el("button", {
      type: "button", class: "botao-secundario botao-pequeno", texto: "Repor",
      "aria-label": `Repor estoque de ${produto.nome}`,
      onclick: () => abrirReposicao(produto)
    })));
}

function filtrar() {
  const termo = busca.value.trim().toLowerCase();
  const achados = produtos.filter((p) => p.nome.toLowerCase().includes(termo));
  if (achados.length === 0) {
    paginacao.replaceChildren();
    corpo.replaceChildren(el("tr", {}, el("td", {
      colspan: 6, class: "linha-estado", texto: `Nenhum produto encontrado para "${busca.value}".`
    })));
  } else {
    mostrarPagina(corpo, paginacao, achados, montarLinha);
  }
}

async function carregar() {
  produtos = await carregarTabela(corpo, 6,
    async () => (await api("GET", "produtos")).sort((a, b) => a.nome.localeCompare(b.nome, "pt-BR")),
    montarLinha,
    "Nenhum produto cadastrado. Clique em Cadastrar produto.",
    paginacao);
  if (busca.value) filtrar();
}

busca.addEventListener("input", filtrar);

// Cadastro

const formProduto = document.getElementById("form-produto");
const campoPreco = document.getElementById("campo-preco");
const inputPreco = document.getElementById("produto-preco");
mascaraMoeda(inputPreco);

// Material interno não tem preço (igual à regra do Produto.Validar no C#)
function mostrarPreco() {
  const venda = formProduto.elements.tipo.value === "venda";
  campoPreco.hidden = !venda;
  inputPreco.required = venda;
}

formProduto.addEventListener("change", mostrarPreco);

document.getElementById("botao-novo-produto").addEventListener("click", () => {
  mostrarPreco();
  abrirDialogo("dialogo-produto");
});

ligarFormulario(formProduto, async (dados) => {
  const tipo = dados.get("tipo");
  const produto = await api("POST", "produtos", {
    nome: dados.get("nome"),
    tipo,
    unidade: dados.get("unidade"),
    precoVenda: tipo === "venda" ? valorDaMascara(dados.get("precoVenda")) : null,
    qtdEstoque: Number(dados.get("qtdEstoque")),
    qtdMinima: Number(dados.get("qtdMinima"))
  });
  avisar(`${produto.nome} cadastrado, ${emEstoque(produto)} em estoque.`);
  await carregar();
});

// Reposição

let produtoRepor = null;

function abrirReposicao(produto) {
  produtoRepor = produto;
  document.getElementById("repor-produto").textContent = `${produto.nome}: ${emEstoque(produto)} em estoque agora.`;
  abrirDialogo("dialogo-repor");
}

ligarFormulario(document.getElementById("form-repor"), async (dados) => {
  const produto = await api("POST", `produtos/${produtoRepor.produtoId}/repor`, {
    quantidade: Number(dados.get("quantidade"))
  });
  avisar(`${produto.nome}: estoque agora é ${emEstoque(produto)}.`);
  await carregar();
});

carregar();
