USE checkpoint;

-- 6. CONSULTAS COM GROUP BY (3 delas com HAVING)

-- 6.1 Quantidade de sessões, minutos jogados e valor arrecadado por tipo de estação
SELECT e.tipo, COUNT(*) AS qtdSessoes, SUM(s.minutos) AS totalMinutos, SUM(s.valor) AS totalArrecadado
	FROM sessao AS s
	INNER JOIN estacao AS e ON e.estacaoId = s.FK_EstacaoId
	GROUP BY e.tipo;

-- 6.2 Minutos de jogo concedidos pelo projeto social, por tipo de obra (só resumos aprovados)
SELECT o.tipo, COUNT(*) AS resumosAprovados, SUM(r.minutosConcedidos) AS minutosConcedidos
	FROM resumo AS r
	INNER JOIN obra AS o ON o.obraId = r.FK_ObraId
	WHERE r.status = 'aprovado'
	GROUP BY o.tipo
	ORDER BY minutosConcedidos DESC;

-- 6.3 (HAVING) Faturamento por forma de pagamento, só as formas que passaram de R$ 200,00
SELECT formaPagamento, COUNT(*) AS qtdComandas, SUM(valorTotal) AS faturamento
	FROM comanda
	WHERE status = 'fechada'
	GROUP BY formaPagamento
	HAVING SUM(valorTotal) > 200
	ORDER BY faturamento DESC;

-- 6.4 (HAVING) Educadores que avaliaram mais de 4 resumos
SELECT p.nome AS avaliador, COUNT(*) AS qtdAvaliados,
	-- Cada resumo aprovado vale 1 e os outros 0, então o SUM conta os aprovados.
	SUM(CASE WHEN r.status = 'aprovado' THEN 1 ELSE 0 END) AS aprovados
	FROM resumo AS r
	INNER JOIN funcionario AS f ON f.funcionarioId = r.FK_FuncionarioId
	INNER JOIN pessoa AS p ON p.pessoaId = f.FK_PessoaId
	GROUP BY r.FK_FuncionarioId, p.nome
	HAVING COUNT(*) > 4
	ORDER BY qtdAvaliados DESC;

-- 6.5 (HAVING) Produtos com mais de 3 unidades vendidas
SELECT pr.nome, SUM(i.quantidade) AS unidadesVendidas, SUM(i.quantidade * i.precoUnitario) AS totalVendido
	FROM itemComanda AS i
	INNER JOIN produto AS pr ON pr.produtoId = i.FK_ProdutoId
	GROUP BY i.FK_ProdutoId, pr.nome
	HAVING SUM(i.quantidade) > 3
	ORDER BY unidadesVendidas DESC;

-- 7. CONSULTAS COM SUBCONSULTA

-- 7.1 Saldo de horas sociais de cada criança (regra do projeto: o saldo é calculado, não armazenado)
--     minutos ganhos (resumos + resoluções aprovados) - minutos usados em sessões de crédito social
--     Pesquisei para estudar: COALESCE(valor, 0) troca NULL por 0. Sem nenhuma linha para somar, o SUM
--     devolve NULL, e NULL + 45 dá NULL. Com o COALESCE fica 0 + 45 = 45.
SELECT p.nome,
	(SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resumo
		WHERE FK_ClienteId = c.clienteId AND status = 'aprovado') +
	(SELECT COALESCE(SUM(minutosConcedidos), 0) FROM resolucao
		WHERE FK_ClienteId = c.clienteId AND status = 'aprovado') AS minutosGanhos,
	(SELECT COALESCE(SUM(minutos), 0) FROM sessao
		WHERE FK_ClienteId = c.clienteId AND origem = 'credito social') AS minutosUsados
	FROM cliente AS c
	INNER JOIN pessoa AS p ON p.pessoaId = c.FK_PessoaId
	WHERE c.FK_ResponsavelId IS NOT NULL
	ORDER BY minutosGanhos DESC;

-- 7.2 Produtos à venda com preço acima da média dos produtos à venda
SELECT nome, precoVenda
	FROM produto
	WHERE tipo = 'venda'
		AND precoVenda > (SELECT AVG(precoVenda) FROM produto WHERE tipo = 'venda')
	ORDER BY precoVenda DESC;

-- 7.3 Obras do acervo que nenhuma criança resumiu ainda
SELECT titulo, tipo, faixaEtaria
	FROM obra
	WHERE obraId NOT IN (SELECT FK_ObraId FROM resumo)
	ORDER BY tipo, titulo;

-- 7.4 Clientes que já fizeram mais sessões do que a média de sessões por cliente
SELECT p.nome, COUNT(*) AS qtdSessoes
	FROM sessao AS s
	INNER JOIN cliente AS c ON c.clienteId = s.FK_ClienteId
	INNER JOIN pessoa AS p ON p.pessoaId = c.FK_PessoaId
	GROUP BY s.FK_ClienteId, p.nome
	HAVING COUNT(*) > (SELECT AVG(r.total) FROM
		(SELECT COUNT(*) AS total FROM sessao GROUP BY FK_ClienteId) AS r)
	ORDER BY qtdSessoes DESC;

-- 7.5 Comandas fechadas com valor acima da média das comandas fechadas
SELECT comandaId, dataHoraAbertura, valorTotal, formaPagamento
	FROM comanda
	WHERE status = 'fechada'
		AND valorTotal > (SELECT AVG(valorTotal) FROM comanda WHERE status = 'fechada')
	ORDER BY valorTotal DESC;

-- 8. CONSULTAS COM JOIN

-- 8.1 Clientes menores de idade com o responsável que autorizou e o parentesco
-- Pesquisei para estudar: TIMESTAMPDIFF(YEAR, nascimento, data) calcula a idade em anos completos.
-- CURDATE() é a data de hoje.
SELECT pc.nome AS crianca, TIMESTAMPDIFF(YEAR, pc.dataNasc, CURDATE()) AS idade,
	pr.nome AS responsavel, c.parentesco, c.dataAutorizacao, pr.telefone AS telefoneResponsavel
	FROM cliente AS c
	INNER JOIN pessoa AS pc ON pc.pessoaId = c.FK_PessoaId
	INNER JOIN responsavel AS r ON r.responsavelId = c.FK_ResponsavelId
	INNER JOIN pessoa AS pr ON pr.pessoaId = r.FK_PessoaId
	ORDER BY pr.nome, pc.nome;

-- 8.2 Endereço completo de cada pessoa (LEFT JOIN: mostra também quem não tem endereço)
SELECT p.nome, en.logradouro, en.numero, b.nome AS bairro, ci.nome AS cidade, es.uf, en.cep
	FROM pessoa AS p
	LEFT JOIN endereco AS en ON en.enderecoId = p.FK_EnderecoId
	LEFT JOIN bairro AS b ON b.bairroId = en.FK_BairroId
	LEFT JOIN cidade AS ci ON ci.cidadeId = b.FK_CidadeId
	LEFT JOIN estado AS es ON es.estadoId = ci.FK_EstadoId
	ORDER BY ci.nome, b.nome, p.nome;

-- 8.3 Resumos aprovados: quem leu, qual obra, quem avaliou e quantos minutos ganhou
SELECT pc.nome AS crianca, o.titulo, o.tipo, pf.nome AS avaliador, r.minutosConcedidos, r.dataHoraEntrega
	FROM resumo AS r
	INNER JOIN cliente AS c ON c.clienteId = r.FK_ClienteId
	INNER JOIN pessoa AS pc ON pc.pessoaId = c.FK_PessoaId
	INNER JOIN obra AS o ON o.obraId = r.FK_ObraId
	INNER JOIN funcionario AS f ON f.funcionarioId = r.FK_FuncionarioId
	INNER JOIN pessoa AS pf ON pf.pessoaId = f.FK_PessoaId
	WHERE r.status = 'aprovado'
	ORDER BY r.dataHoraEntrega;

-- 8.4 Itens vendidos em cada comanda, com cliente (ou "sem cadastro"), caixa e subtotal
--     comanda de balcão não tem cliente: o LEFT JOIN traz o nome NULL e o COALESCE (ver 7.1) troca por "sem cadastro"
SELECT co.comandaId, COALESCE(p.nome, 'sem cadastro') AS cliente, co.FK_CaixaId AS caixa,
	pr.nome AS produto, i.quantidade, i.precoUnitario, i.quantidade * i.precoUnitario AS subtotal
	FROM itemComanda AS i
	INNER JOIN comanda AS co ON co.comandaId = i.FK_ComandaId
	INNER JOIN produto AS pr ON pr.produtoId = i.FK_ProdutoId
	LEFT JOIN cliente AS c ON c.clienteId = co.FK_ClienteId
	LEFT JOIN pessoa AS p ON p.pessoaId = c.FK_PessoaId
	ORDER BY co.comandaId;

-- 8.5 Sessões de crianças em jogos acima da classificação indicativa da idade delas
--     (cruza sessão, cliente, pessoa, estação e jogos instalados na estação)
--     Serve de auditoria: o back-end barra essas sessões, então o resultado esperado é vazio.
-- Pesquisei para estudar: TIMESTAMPDIFF (ver 8.1), com a idade no dia da sessão.
SELECT pc.nome AS crianca, TIMESTAMPDIFF(YEAR, pc.dataNasc, s.dataHoraInicio) AS idade,
	e.numero AS estacao, j.nome AS jogoInstalado, j.classificacaoIndicativa
	FROM sessao AS s
	INNER JOIN cliente AS c ON c.clienteId = s.FK_ClienteId
	INNER JOIN pessoa AS pc ON pc.pessoaId = c.FK_PessoaId
	INNER JOIN estacao AS e ON e.estacaoId = s.FK_EstacaoId
	INNER JOIN jogoEstacao AS je ON je.FK_EstacaoId = e.estacaoId
	INNER JOIN jogo AS j ON j.jogoId = je.FK_JogoId
	WHERE c.FK_ResponsavelId IS NOT NULL
		AND j.classificacaoIndicativa > TIMESTAMPDIFF(YEAR, pc.dataNasc, s.dataHoraInicio)
	ORDER BY pc.nome;