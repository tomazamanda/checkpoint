CREATE DATABASE IF NOT EXISTS checkpoint;
USE checkpoint;
-- DROP DATABASE checkpoint;

-- ENDEREÇO

CREATE TABLE IF NOT EXISTS estado(
	estadoId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    uf CHAR(2) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS cidade(
	cidadeId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    FK_EstadoId INT NOT NULL,
    FOREIGN KEY (FK_EstadoId) REFERENCES estado(estadoId)
);

CREATE TABLE IF NOT EXISTS bairro(
	bairroId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    FK_CidadeId INT NOT NULL,
    FOREIGN KEY (FK_CidadeId) REFERENCES cidade(cidadeId)
);

CREATE TABLE IF NOT EXISTS endereco(
	enderecoId INT PRIMARY KEY AUTO_INCREMENT,
    logradouro VARCHAR(150) NOT NULL,
    numero VARCHAR(10),
    complemento VARCHAR(100),
    cep CHAR(8) NOT NULL,
    referencia VARCHAR(150),
    FK_BairroId INT NOT NULL,
    FOREIGN KEY (FK_BairroId) REFERENCES bairro(bairroId)
);

-- PESSOAS (genérica + especializações)

CREATE TABLE IF NOT EXISTS pessoa(
	pessoaId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    cpf CHAR(11) NOT NULL UNIQUE,
    dataNasc DATE NOT NULL,
    telefone VARCHAR(15),
    email VARCHAR(100) UNIQUE,      -- pode ser NULL; quando informado, não repete
    FK_EnderecoId INT,              -- pode ser NULL pois a relação é (0,1)
    FOREIGN KEY (FK_EnderecoId) REFERENCES endereco(enderecoId)
);

-- Especialização de pessoa: PK + FK_PessoaId UNIQUE (uma pessoa é no máximo 1 responsável)
CREATE TABLE IF NOT EXISTS responsavel(
	responsavelId INT PRIMARY KEY AUTO_INCREMENT,
    FK_PessoaId INT NOT NULL UNIQUE,
    FOREIGN KEY (FK_PessoaId) REFERENCES pessoa(pessoaId)
);

CREATE TABLE IF NOT EXISTS cliente(
	clienteId INT PRIMARY KEY AUTO_INCREMENT,
    dataCadastro DATE NOT NULL,
    parentesco VARCHAR(20),         -- atributo do relacionamento "autoriza"
    dataAutorizacao DATE,           -- atributo do relacionamento "autoriza"
    FK_PessoaId INT NOT NULL UNIQUE,
    FK_ResponsavelId INT,           -- NULL para cliente adulto
    FOREIGN KEY (FK_PessoaId) REFERENCES pessoa(pessoaId),
    FOREIGN KEY (FK_ResponsavelId) REFERENCES responsavel(responsavelId)
);

CREATE TABLE IF NOT EXISTS funcionario(
	funcionarioId INT PRIMARY KEY AUTO_INCREMENT,
    cargo VARCHAR(50) NOT NULL,
    dataAdmissao DATE NOT NULL,
    senhaHash VARCHAR(100),         -- hash PBKDF2 da senha de login; NULL = não entra no sistema
    FK_PessoaId INT NOT NULL UNIQUE,
    FOREIGN KEY (FK_PessoaId) REFERENCES pessoa(pessoaId)
);

-- JOGOS

CREATE TABLE IF NOT EXISTS estacao(
	estacaoId INT PRIMARY KEY AUTO_INCREMENT,
    numero INT NOT NULL UNIQUE,
    tipo ENUM('computador','videogame') NOT NULL,
    plataforma VARCHAR(30) NOT NULL,
    status ENUM('disponivel','em uso','manutencao') NOT NULL DEFAULT 'disponivel',
    precoHora DECIMAL(6,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS jogo(
	jogoId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    genero VARCHAR(50),
    classificacaoIndicativa TINYINT NOT NULL    -- 0, 10, 12, 14, 16, 18
);

-- Tabela associativa gerada pelo relacionamento N:N (estacao possui jogo)
CREATE TABLE IF NOT EXISTS jogoEstacao(
	jogoEstacaoId INT PRIMARY KEY AUTO_INCREMENT,
    dataInstalacao DATE,
    FK_EstacaoId INT NOT NULL,
    FK_JogoId INT NOT NULL,
    -- Pesquisei para estudar: UNIQUE com duas colunas. O que não pode repetir é o par estação + jogo.
    -- itemComanda, resumo e resolucao usam o mesmo.
    UNIQUE (FK_EstacaoId, FK_JogoId),           -- o mesmo jogo não é instalado 2x na mesma estação
    FOREIGN KEY (FK_EstacaoId) REFERENCES estacao(estacaoId),
    FOREIGN KEY (FK_JogoId) REFERENCES jogo(jogoId)
);

-- VENDAS E CAIXA

CREATE TABLE IF NOT EXISTS caixa(
	caixaId INT PRIMARY KEY AUTO_INCREMENT,
    dataHoraAbertura DATETIME NOT NULL,
    valorAbertura DECIMAL(10,2) NOT NULL,
    dataHoraFechamento DATETIME,
    valorFechamento DECIMAL(10,2),
    status ENUM('aberto','fechado') NOT NULL DEFAULT 'aberto',
    FK_FuncionarioId INT NOT NULL,
    FOREIGN KEY (FK_FuncionarioId) REFERENCES funcionario(funcionarioId)
);

CREATE TABLE IF NOT EXISTS comanda(
	comandaId INT PRIMARY KEY AUTO_INCREMENT,
    dataHoraAbertura DATETIME NOT NULL,
    dataHoraFechamento DATETIME,
    valorTotal DECIMAL(10,2),
    formaPagamento ENUM('dinheiro','pix','debito','credito'),
    status ENUM('aberta','fechada','cancelada') NOT NULL DEFAULT 'aberta',
    FK_ClienteId INT,               -- NULL quando quem compra não tem cadastro
    FK_CaixaId INT NOT NULL,
    FOREIGN KEY (FK_ClienteId) REFERENCES cliente(clienteId),
    FOREIGN KEY (FK_CaixaId) REFERENCES caixa(caixaId)
);

CREATE TABLE IF NOT EXISTS sessao(
	sessaoId INT PRIMARY KEY AUTO_INCREMENT,
    dataHoraInicio DATETIME NOT NULL,
    dataHoraFim DATETIME,
    minutos INT,
    origem ENUM('paga','credito social') NOT NULL,
    valor DECIMAL(8,2),
    FK_ClienteId INT NOT NULL,
    FK_EstacaoId INT NOT NULL,
    FK_ComandaId INT,               -- NULL na sessão por crédito social
    FOREIGN KEY (FK_ClienteId) REFERENCES cliente(clienteId),
    FOREIGN KEY (FK_EstacaoId) REFERENCES estacao(estacaoId),
    FOREIGN KEY (FK_ComandaId) REFERENCES comanda(comandaId)
);

CREATE TABLE IF NOT EXISTS produto(
	produtoId INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    tipo ENUM('venda','material interno') NOT NULL,
    unidade VARCHAR(10) NOT NULL,
    precoVenda DECIMAL(8,2),        -- NULL para material interno
    qtdEstoque INT NOT NULL DEFAULT 0,
    qtdMinima INT NOT NULL DEFAULT 0
);

-- Tabela associativa gerada pelo relacionamento N:N (comanda contém produto)
CREATE TABLE IF NOT EXISTS itemComanda(
	itemComandaId INT PRIMARY KEY AUTO_INCREMENT,
    quantidade INT NOT NULL,
    precoUnitario DECIMAL(8,2) NOT NULL,        -- preço no momento da venda
    FK_ComandaId INT NOT NULL,
    FK_ProdutoId INT NOT NULL,
    UNIQUE (FK_ComandaId, FK_ProdutoId),
    FOREIGN KEY (FK_ComandaId) REFERENCES comanda(comandaId),
    FOREIGN KEY (FK_ProdutoId) REFERENCES produto(produtoId)
);

-- PROJETO SOCIAL

CREATE TABLE IF NOT EXISTS obra(
	obraId INT PRIMARY KEY AUTO_INCREMENT,
    titulo VARCHAR(150) NOT NULL,
    tipo ENUM('livro','gibi','manga') NOT NULL,
    autor VARCHAR(100),
    editora VARCHAR(100),
    faixaEtaria TINYINT NOT NULL,
    qtdPaginas INT,
    qtdExemplares INT NOT NULL DEFAULT 1,
    minutosRecompensa INT NOT NULL
);

CREATE TABLE IF NOT EXISTS desafio(
	desafioId INT PRIMARY KEY AUTO_INCREMENT,
    titulo VARCHAR(150) NOT NULL,
    enunciado TEXT NOT NULL,
    area ENUM('logica','programacao') NOT NULL,
    nivel ENUM('facil','medio','dificil') NOT NULL,
    gabarito TEXT,
    minutosRecompensa INT NOT NULL
);

-- Tabela da entidade associativa resumo (cliente x obra)
CREATE TABLE IF NOT EXISTS resumo(
	resumoId INT PRIMARY KEY AUTO_INCREMENT,
    dataHoraInicio DATETIME NOT NULL,
    dataHoraEntrega DATETIME,
    fotoUrl VARCHAR(255),
    status ENUM('em andamento','entregue','aprovado','recusado') NOT NULL DEFAULT 'em andamento',
    minutosConcedidos INT,          -- NULL até a avaliação
    observacao TEXT,
    FK_ClienteId INT NOT NULL,
    FK_ObraId INT NOT NULL,
    FK_FuncionarioId INT,           -- NULL até a avaliação
    UNIQUE (FK_ClienteId, FK_ObraId),           -- não ganha horas 2x pela mesma obra
    FOREIGN KEY (FK_ClienteId) REFERENCES cliente(clienteId),
    FOREIGN KEY (FK_ObraId) REFERENCES obra(obraId),
    FOREIGN KEY (FK_FuncionarioId) REFERENCES funcionario(funcionarioId)
);

-- Tabela da entidade associativa resolucao (cliente x desafio)
CREATE TABLE IF NOT EXISTS resolucao(
	resolucaoId INT PRIMARY KEY AUTO_INCREMENT,
    dataHoraInicio DATETIME NOT NULL,
    dataHoraEntrega DATETIME,
    fotoUrl VARCHAR(255),
    status ENUM('em andamento','entregue','aprovado','recusado') NOT NULL DEFAULT 'em andamento',
    minutosConcedidos INT,
    observacao TEXT,
    FK_ClienteId INT NOT NULL,
    FK_DesafioId INT NOT NULL,
    FK_FuncionarioId INT,
    UNIQUE (FK_ClienteId, FK_DesafioId),        -- não ganha horas 2x pelo mesmo desafio
    FOREIGN KEY (FK_ClienteId) REFERENCES cliente(clienteId),
    FOREIGN KEY (FK_DesafioId) REFERENCES desafio(desafioId),
    FOREIGN KEY (FK_FuncionarioId) REFERENCES funcionario(funcionarioId)
);

SHOW TABLES;