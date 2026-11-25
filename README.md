# Checkpoint

Sistema de gestão de uma lan house com projeto social. Trabalho de ADS (IFRO) para Programação Orientada a Objetos (API em C# e front-end) e Banco de Dados II (modelos conceitual, lógico e físico em MySQL).

O atendente abre e fecha o caixa, lança produtos e sessões em comandas e cadastra clientes. No projeto social, resumos de livros e desafios de lógica viram minutos de jogo.

## O que tem aqui

| Pasta | Conteúdo |
|---|---|
| `banco/conceitual/` e `banco/logico/` | Modelos no brModelo, com as imagens exportadas e prints do conceitual |
| `banco/fisico/` | Criação do banco (20 tabelas), dados de exemplo e consultas |
| `checkpoint/` | API em C# (.NET 10, ASP.NET Core, ADO.NET e DAO) e front-end em HTML, CSS e JavaScript |

## Regras de negócio

As regras ficam no back-end e aparecem como mensagem na tela. As principais:

- Sessão barrada se a estação tiver jogo acima da classificação indicativa do cliente.
- Cliente menor de idade só é cadastrado com responsável.
- Limite de 300 minutos de crédito social por semana, e só educador social ou gerente avaliam as atividades.

As demais estão no documento do projeto.

## O que ficou para depois

Por falta de tempo, ficaram para uma próxima versão. Os principais:

- tela só para o caixa e perfil de administrador para controlar os usuários;
- editar e excluir registros, e cadastrar pelas telas o que hoje entra pelo SQL;
- controle do tempo na própria máquina, que libera e bloqueia o computador.

A lista completa está no documento do projeto.

## Como rodar

1. No MySQL, rode `banco/fisico/checkpoint.sql` e depois `banco/fisico/checkpointInserts.sql`.
2. Crie `checkpoint/checkpoint/Database/Conexao.cs` (abaixo).
3. Abra `checkpoint/checkpoint.slnx` no Visual Studio e aperte F5. O sistema abre em `http://localhost:5080`.

As regras avisam por exceção. Com F5, o Visual Studio pausa nelas: clique em **Continuar** ou rode com **Ctrl+F5**.

**Login** (senha `checkpoint123` para todos):

| E-mail | Cargo |
|---|---|
| alice.batista111@example.com | educador social |
| claudia.oliveira129@example.com | atendente |

### Conexao.cs

Fica fora do Git porque guarda a senha do MySQL. Troque `SUA_SENHA`:

```csharp
using MySql.Data.MySqlClient;

namespace checkpoint.Database
{
    public static class Conexao
    {
        private const string strconexao =
            "server=localhost;uid=root;pwd=SUA_SENHA;database=checkpoint;port=3306";

        public static MySqlConnection Conectar()
        {
            MySqlConnection conexao = new MySqlConnection(strconexao);
            conexao.Open();
            return conexao;
        }
    }
}
```

## Comentários

O código tem comentários de estudo. O que foi além da aula está marcado com "Pesquisei para estudar".
