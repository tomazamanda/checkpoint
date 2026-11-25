using checkpoint.Database;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    // Parte comum de resumo e resolucao (as duas tabelas têm as mesmas colunas de atividade).
    // Mesmo papel do PessoaDAO: evita repetir código em ResumoDAO e ResolucaoDAO.
    public static class AtividadeSocialDAO
    {
        public static void Parametros(MySqlCommand cmd, AtividadeSocial atividade)
        {
            cmd.Parameters.AddWithValue("@dataHoraInicio", atividade.DataHoraInicio);
            cmd.Parameters.AddWithValue("@dataHoraEntrega", Converte.ParaBanco(atividade.DataHoraEntrega));
            cmd.Parameters.AddWithValue("@fotoUrl", Converte.ParaBanco(atividade.FotoUrl));
            cmd.Parameters.AddWithValue("@status", atividade.Status);
            cmd.Parameters.AddWithValue("@minutosConcedidos", Converte.ParaBanco(atividade.MinutosConcedidos));
            cmd.Parameters.AddWithValue("@observacao", Converte.ParaBanco(atividade.Observacao));
            cmd.Parameters.AddWithValue("@clienteId", atividade.Cliente.ClienteId);
            if (atividade.Avaliador == null)   // ainda não foi avaliada
                cmd.Parameters.AddWithValue("@funcionarioId", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@funcionarioId", atividade.Avaliador.FuncionarioId);
        }

        public static void Mapear(MySqlDataReader dr, AtividadeSocial atividade)
        {
            atividade.DataHoraInicio = Convert.ToDateTime(dr["dataHoraInicio"]);
            atividade.DataHoraEntrega = Converte.DataHora(dr, "dataHoraEntrega");
            atividade.FotoUrl = Converte.Texto(dr, "fotoUrl");
            atividade.Status = dr["status"].ToString();
            atividade.MinutosConcedidos = Converte.Inteiro(dr, "minutosConcedidos");
            atividade.Observacao = Converte.Texto(dr, "observacao");
            atividade.Cliente = new Cliente();
            atividade.Cliente.ClienteId = Convert.ToInt32(dr["FK_ClienteId"]);
            atividade.Cliente.Nome = dr["nomeCliente"].ToString();

            int? funcionarioId = Converte.Inteiro(dr, "FK_FuncionarioId");
            if (funcionarioId != null)
            {
                atividade.Avaliador = new Funcionario();
                atividade.Avaliador.FuncionarioId = funcionarioId.Value;
            }
        }
    }
}
