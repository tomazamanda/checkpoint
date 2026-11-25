// Funções usadas por todas as telas: falar com a API, formatar valores e montar pedaços da página.

const naTelaDeLogin = location.pathname === "/login.html";

export async function api(metodo, caminho, corpo) {
  const opcoes = { method: metodo, headers: {} };
  if (corpo instanceof FormData) {
    opcoes.body = corpo;   // com arquivo (foto): o navegador monta o envio e o cabeçalho sozinho
  } else if (corpo !== undefined) {
    opcoes.headers["Content-Type"] = "application/json";
    opcoes.body = JSON.stringify(corpo);
  }

  let resposta;
  try {
    resposta = await fetch("/api/" + caminho, opcoes);
  } catch {
    throw new Error("Sem conexão com a API. Confira se o projeto está rodando no Visual Studio.");
  }

  // Sessão expirada (ou sem login): manda para o login e volta para esta tela depois
  if (resposta.status === 401 && !naTelaDeLogin) {
    location.href = "/login.html?ReturnUrl=" + encodeURIComponent(location.pathname + location.search);
    return new Promise(() => {});   // a página está saindo; nada mais roda
  }

  if (resposta.status === 204) return null;   // ex.: nenhum caixa aberto

  const dados = await resposta.json().catch(() => null);
  if (!resposta.ok) {
    // a API manda { mensagem } com o texto do throw new Exception(...) do C#
    throw new Error(dados?.mensagem ?? `A API respondeu com erro ${resposta.status}.`);
  }
  return dados;
}

// Formatação 

const formatoMoeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export function moeda(valor) {
  return formatoMoeda.format(valor ?? 0);
}

// Máscara de dinheiro: o campo aceita só dígitos, lidos como centavos, digitar 15050 mostra "150,50"; a vírgula fica sempre nas duas últimas casas.
const formatoValor = new Intl.NumberFormat("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const MAX_DIGITOS = 9;   // até 9.999.999,99

export function mascaraMoeda(campo) {
  const aplicar = () => {
    // tira os zeros da esquerda, mas mantém um zero se só houver zeros (gaveta vazia = 0,00)
    const digitos = campo.value.replace(/\D/g, "").replace(/^0+(?=\d)/, "").slice(0, MAX_DIGITOS);
    campo.value = digitos === "" ? "" : formatoValor.format(Number(digitos) / 100);
  };
  campo.addEventListener("input", aplicar);
  aplicar();
}

/* Máscara de CPF: 000.000.000-00, só dígitos e no máximo 11. O back-end tira a pontuação.
   Máscara de quantidade: só dígitos, até 4 (máximo 9999), sem zero à esquerda.
   Um número gigante não cabe no int do C# e voltaria como erro sem explicação. */
export function mascaraQuantidade(campo) {
  campo.maxLength = 4;
  campo.addEventListener("input", () => {
    campo.value = campo.value.replace(/\D/g, "").replace(/^0+(?=\d)/, "").slice(0, 4);
  });
}

export function mascaraCpf(campo) {
  campo.maxLength = 14;
  campo.addEventListener("input", () => {
    const d = campo.value.replace(/\D/g, "").slice(0, 11);
    campo.value = d
      .replace(/^(\d{3})(\d)/, "$1.$2")
      .replace(/^(\d{3})\.(\d{3})(\d)/, "$1.$2.$3")
      .replace(/\.(\d{3})(\d{1,2})$/, ".$1-$2");
  });
}

export function valorDaMascara(texto) {
  const digitos = String(texto ?? "").replace(/\D/g, "");
  return digitos === "" ? 0 : Number(digitos) / 100;
}

export function dataHora(texto) {
  if (!texto) return "";
  const formatada = new Date(texto).toLocaleString("pt-BR", {
    day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit"
  });
  return formatada.replace(", ", ",\u00A0");   // espaço que não quebra: data e hora na mesma linha
}

// DateOnly chega como "2017-10-16". new Date() leria em UTC e voltaria um dia no fuso de Rondônia, por isso a data é montada à mão.
export function data(texto) {
  if (!texto) return "";
  const [ano, mes, dia] = texto.split("-");
  return `${dia}/${mes}/${ano}`;
}

export function idade(dataNasc) {
  const [ano, mes, dia] = dataNasc.split("-").map(Number);
  const hoje = new Date();
  let anos = hoje.getFullYear() - ano;
  const fezAniversario = hoje.getMonth() + 1 > mes || (hoje.getMonth() + 1 === mes && hoje.getDate() >= dia);
  if (!fezAniversario) anos--;
  return anos;
}

// "1 comanda", "2 comandas"
export function plural(quantidade, singular, plural) {
  return `${quantidade} ${quantidade === 1 ? singular : plural}`;
}

const pluralUnidade = { un: "un", "porção": "porções", par: "pares" };

export function emEstoque(produto) {
  const unidade = produto.unidade;
  return plural(produto.qtdEstoque, unidade, pluralUnidade[unidade] ?? unidade + "s");
}

export function minutosEmHoras(minutos) {
  const horas = Math.floor(minutos / 60);
  const resto = String(minutos % 60).padStart(2, "0");
  return `${horas}h${resto}`;
}

// Montar a página 

// Cria um elemento. Texto sempre entra como texto (nunca como HTML), então um nome digitado com tags não vira código na página.
export function el(tag, atributos = {}, ...filhos) {
  const elemento = document.createElement(tag);
  for (const [nome, valor] of Object.entries(atributos)) {
    if (nome === "texto") elemento.textContent = valor;
    else if (nome.startsWith("on")) elemento.addEventListener(nome.slice(2), valor);
    else if (valor !== false && valor != null) elemento.setAttribute(nome, valor === true ? "" : valor);
  }
  for (const filho of filhos) {
    if (filho != null) elemento.append(filho);
  }
  return elemento;
}

export function celula(texto, classe) {
  return el("td", { texto: texto ?? "", class: classe });
}

function linhaUnica(corpo, colunas, texto, classe) {
  const td = el("td", { colspan: colunas, class: "linha-estado " + (classe ?? ""), texto });
  corpo.replaceChildren(el("tr", {}, td));
}

const POR_PAGINA = 10;

// Paginação feita no navegador: a API devolve a lista inteira e a tabela mostra 0 linhas por vez, com os botões Anterior e Próxima no elemento "controles".
export function mostrarPagina(corpo, controles, lista, montarLinha, pagina = 1) {
  const totalPaginas = Math.max(1, Math.ceil(lista.length / POR_PAGINA));
  pagina = Math.min(Math.max(pagina, 1), totalPaginas);
  const inicio = (pagina - 1) * POR_PAGINA;
  corpo.replaceChildren(...lista.slice(inicio, inicio + POR_PAGINA).map(montarLinha));

  if (totalPaginas === 1) {
    controles.replaceChildren();
    return;
  }
  const irPara = (numero) => () => mostrarPagina(corpo, controles, lista, montarLinha, numero);
  controles.replaceChildren(
    el("button", {
      type: "button", class: "botao-secundario botao-pequeno", texto: "Anterior",
      disabled: pagina === 1, onclick: irPara(pagina - 1)
    }),
    el("span", { texto: `Página ${pagina} de ${totalPaginas} · ${lista.length} no total` }),
    el("button", {
      type: "button", class: "botao-secundario botao-pequeno", texto: "Próxima",
      disabled: pagina === totalPaginas, onclick: irPara(pagina + 1)
    }));
}

// Preenche um <tbody> mostrando os três estados: carregando, vazio e erro. Com "controles", a lista é mostrada em páginas.
export async function carregarTabela(corpo, colunas, buscar, montarLinha, textoVazio, controles) {
  linhaUnica(corpo, colunas, "Carregando...");
  controles?.replaceChildren();
  try {
    const lista = await buscar();
    if (lista.length === 0) linhaUnica(corpo, colunas, textoVazio);
    else if (controles) mostrarPagina(corpo, controles, lista, montarLinha);
    else corpo.replaceChildren(...lista.map(montarLinha));
    return lista;
  } catch (erro) {
    linhaUnica(corpo, colunas, "Erro: " + erro.message, "linha-erro");
    return [];
  }
}

export function preencherSelect(select, lista, valor, texto, primeiraOpcao) {
  const opcoes = lista.map((item) => el("option", { value: valor(item), texto: texto(item) }));
  if (primeiraOpcao) opcoes.unshift(el("option", { value: "", texto: primeiraOpcao }));
  select.replaceChildren(...opcoes);
}

/* Lista com busca: a pessoa digita ("Play", "Bruno") e a lista mostra só as opções com esse texto.
 O <select> continua no formulário, escondido, guardando o id escolhido: FormData, required, select.value e o evento "change" das telas funcionam como antes.
 A lista é feita aqui, e não com <datalist>, porque o Chrome abre o datalist fora do lugar dentro de <dialog>. */
export function selectComBusca(select) {
  const lista = el("ul", { id: select.id + "-opcoes", class: "busca-opcoes", role: "listbox", hidden: true });
  const campo = el("input", {
    id: select.id + "-busca", role: "combobox", autocomplete: "off",
    "aria-controls": lista.id, "aria-expanded": "false", "aria-autocomplete": "list"
  });
  const caixa = el("div", { class: "select-busca" });
  select.before(caixa);
  caixa.append(campo, lista, select);
  document.querySelector(`label[for="${select.id}"]`).htmlFor = campo.id;
  select.tabIndex = -1;
  select.setAttribute("aria-hidden", "true");

  let opcoes = [];     // { id, texto, busca } de cada opção do select
  let visiveis = [];   // as que contêm o texto digitado
  let ativa = -1;      // a destacada pelas setas do teclado

  // sem acento e em minúsculas: "joao" acha "João"
  const normalizar = (texto) => texto.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();

  // Recria as opções sempre que a tela preenche o select (preencherSelect)
  function copiarOpcoes() {
    opcoes = [...select.options].filter((o) => o.value !== "")
      .map((o) => ({ id: o.value, texto: o.text, busca: normalizar(o.text) }));
    const vazia = select.options[0]?.value === "" ? select.options[0].text : "";
    campo.placeholder = vazia || "Digite para buscar";
    campo.value = "";
    campo.setCustomValidity("");
    fechar();
  }

  function marcar(id) {
    if (select.value === id) return;
    select.value = id;
    select.dispatchEvent(new Event("change", { bubbles: true }));
  }

  function escolher(opcao) {
    campo.value = opcao.texto;
    campo.setCustomValidity("");
    marcar(opcao.id);
    fechar();
  }

  function destacar(indice) {
    ativa = indice;
    [...lista.children].forEach((li, i) => li.setAttribute("aria-selected", String(i === ativa)));
    const li = lista.children[ativa];
    if (li && visiveis.length) {
      campo.setAttribute("aria-activedescendant", li.id);
      li.scrollIntoView({ block: "nearest" });
    } else {
      campo.removeAttribute("aria-activedescendant");
    }
  }

  function abrir() {
    const termo = normalizar(campo.value.trim());
    visiveis = opcoes.filter((o) => o.busca.includes(termo));
    lista.replaceChildren(...(visiveis.length
      ? visiveis.map((o, i) => el("li", {
          id: `${lista.id}-${i}`, role: "option", texto: o.texto,
          // mousedown, e não click: escolhe antes de o campo perder o foco e fechar a lista
          onmousedown: (evento) => { evento.preventDefault(); escolher(o); }
        }))
      : [el("li", { class: "busca-vazia", texto: "Nada encontrado" })]));
    // posição fixa, logo abaixo do campo: a lista passa por cima da borda do diálogo em vez de ser cortada
    const area = campo.getBoundingClientRect();
    Object.assign(lista.style, { left: area.left + "px", top: area.bottom + "px", width: area.width + "px" });
    lista.hidden = false;
    campo.setAttribute("aria-expanded", "true");
    destacar(visiveis.length ? 0 : -1);
  }

  function fechar() {
    lista.hidden = true;
    campo.setAttribute("aria-expanded", "false");
    campo.removeAttribute("aria-activedescendant");
  }

  new MutationObserver(copiarOpcoes).observe(select, { childList: true });
  copiarOpcoes();

  campo.addEventListener("click", abrir);
  campo.addEventListener("blur", fechar);
  campo.addEventListener("input", () => {
    const exata = opcoes.find((o) => o.texto === campo.value);
    marcar(exata?.id ?? "");
    // texto pela metade não pode ser enviado como "vazio" (abriria comanda sem cliente, por exemplo)
    campo.setCustomValidity(campo.value !== "" && !exata ? "Escolha uma opção da lista." : "");
    abrir();
  });
  campo.addEventListener("keydown", (evento) => {
    if (lista.hidden) {
      if (evento.key === "ArrowDown") { abrir(); evento.preventDefault(); }
      return;
    }
    if (evento.key === "ArrowDown") destacar(Math.min(ativa + 1, visiveis.length - 1));
    else if (evento.key === "ArrowUp") destacar(Math.max(ativa - 1, 0));
    else if (evento.key === "Enter" && visiveis[ativa]) escolher(visiveis[ativa]);
    else if (evento.key === "Escape") fechar();   // fecha só a lista, não o diálogo
    else return;
    evento.preventDefault();
  });
  // rolar a página ou o diálogo tiraria a lista de baixo do campo
  addEventListener("scroll", (evento) => { if (evento.target !== lista) fechar(); }, true);
}

export function porNome(a, b) {
  return a.nome.localeCompare(b.nome, "pt-BR");
}

// Avisos e formulários

export function avisar(texto, tipo = "ok") {
  const aviso = document.getElementById("aviso");
  aviso.className = "aviso aviso-" + tipo;
  aviso.textContent = (tipo === "erro" ? "Erro: " : "") + texto;
  aviso.hidden = false;
}

// Liga um formulário à API: impede o envio padrão, trava o botão enquanto espera, mostra o erro dentro do próprio formulário e fecha o diálogo se deu certo.
export function ligarFormulario(formulario, acao) {
  const erro = formulario.querySelector(".erro-form");
  const botao = formulario.querySelector("button[type=submit]");

  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    erro.hidden = true;
    botao.disabled = true;
    try {
      await acao(new FormData(formulario));
      formulario.reset();
      formulario.closest("dialog")?.close();
    } catch (falha) {
      erro.textContent = "Erro: " + falha.message;
      erro.hidden = false;
    } finally {
      botao.disabled = false;
    }
  });
}

export function abrirDialogo(id) {
  const dialogo = document.getElementById(id);
  dialogo.querySelector(".erro-form")?.setAttribute("hidden", "");
  dialogo.showModal();
}

// Pergunta antes de uma ação que não tem volta. Devolve true se a pessoa confirmou.
export function confirmar(pergunta, textoBotao) {
  const dialogo = document.getElementById("confirmacao");
  dialogo.querySelector("p").textContent = pergunta;
  dialogo.querySelector("button[value=sim]").textContent = textoBotao;
  dialogo.returnValue = "";
  dialogo.showModal();
  return new Promise((resolver) => {
    dialogo.addEventListener("close", () => resolver(dialogo.returnValue === "sim"), { once: true });
  });
}

// Botões "Cancelar" dos diálogos
document.addEventListener("click", (evento) => {
  if (evento.target.matches("[data-fechar]")) evento.target.closest("dialog").close();
});

export function numeroOuNulo(texto) {
  return texto === "" || texto == null ? null : Number(texto);
}

// Quem está logado

// Buscado uma vez por tela. As telas usam para decidir o que mostrar (ex.: botão Avaliar).
export const usuarioLogado = naTelaDeLogin ? Promise.resolve(null) : api("GET", "login/eu");

export function podeAvaliar(usuario) {
  return usuario?.cargo === "educador social" || usuario?.cargo === "gerente";
}

// Nome de quem entrou e o botão Sair, no canto do topo de todas as telas
usuarioLogado.then((usuario) => {
  if (!usuario) return;
  const sair = el("button", {
    type: "button", class: "sair", texto: "Sair",
    onclick: async () => {
      await api("POST", "login/sair");
      location.href = "/login.html";
    }
  });
  document.querySelector(".topo").append(
    el("div", { class: "usuario" }, el("span", { texto: usuario.nome }), sair));
}).catch(() => {});
