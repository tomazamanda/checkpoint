import { api, dataHora, plural } from "./api.js";

// Mostra o texto quando a busca dá certo, ou o erro no mesmo lugar
async function escrever(id, buscar, montarTexto) {
  const elemento = document.getElementById(id);
  try {
    elemento.textContent = montarTexto(await buscar());
  } catch (erro) {
    elemento.textContent = "Erro: " + erro.message;
  }
}

document.getElementById("hoje").textContent = new Date().toLocaleDateString("pt-BR", {
  weekday: "long", day: "numeric", month: "long", year: "numeric"
});

escrever("situacao-caixa", () => api("GET", "caixa/aberto"), (caixa) => {
  if (caixa === null) {
    document.getElementById("link-caixa").textContent = "Abrir caixa no balcão";
    return "Nenhum caixa aberto. Abra o caixa para começar a atender.";
  }
  return `Caixa ${caixa.caixaId} · ${caixa.funcionario.nome} · aberto em ${dataHora(caixa.dataHoraAbertura)}`;
});

escrever("resumo-balcao",
  () => Promise.all([api("GET", "comandas/abertas"), api("GET", "sessoes/abertas")]),
  ([comandas, sessoes]) =>
    `${plural(comandas.length, "comanda aberta", "comandas abertas")} · ` +
    `${plural(sessoes.length, "estação em uso", "estações em uso")}`);

escrever("resumo-clientes", () => api("GET", "clientes"),
  (clientes) => plural(clientes.length, "cliente cadastrado", "clientes cadastrados"));

escrever("resumo-social",
  () => Promise.all([api("GET", "resumos/entregues"), api("GET", "resolucoes/entregues")]),
  ([resumos, resolucoes]) => {
    const total = resumos.length + resolucoes.length;
    return total === 0
      ? "Nenhuma atividade esperando avaliação"
      : `${plural(total, "atividade esperando", "atividades esperando")} avaliação`;
  });

escrever("resumo-relatorios", () => api("GET", "produtos/estoque-baixo"),
  (produtos) => produtos.length === 0
    ? "Estoque em dia"
    : `${plural(produtos.length, "produto", "produtos")} no estoque mínimo ou abaixo`);
