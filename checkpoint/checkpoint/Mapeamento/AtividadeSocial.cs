namespace checkpoint.Mapeamento
{
    public abstract class AtividadeSocial
    {
        // Máximo de minutos que a criança ganha por semana (segunda a domingo), somando resumos e desafios, o resumo é feito à mão, dentro da lan house; o limite equilibra o tempo de leitura e o de jogo.
        public const int LimiteSemanalMinutos = 300;

        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraEntrega { get; set; }
        public string FotoUrl { get; set; }
        public int? MinutosConcedidos { get; set; }   // null até a avaliação
        public string Observacao { get; set; }
        public Cliente Cliente { get; set; }
        public Funcionario Avaliador { get; set; }    // null até a avaliação

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value != "em andamento" && value != "entregue" && value != "aprovado" && value != "recusado")
                    throw new Exception("Status de atividade inválido.");
                _status = value;
            }
        }

        protected AtividadeSocial()
        {
            DataHoraInicio = DateTime.Now;
            Status = "em andamento";
        }

        public abstract int MinutosRecompensa();

        // Quem já ganhou o limite da semana não começa outra atividade: o resumo seria feito à toa
        public static void ConferirLimiteSemanal(Cliente cliente, int ganhosNaSemana)
        {
            if (ganhosNaSemana >= LimiteSemanalMinutos)
                throw new Exception($"{cliente.Nome} já ganhou o limite de {LimiteSemanalMinutos} min desta semana. " +
                    "Pode começar outra atividade a partir de segunda-feira.");
        }

        public void Entregar(string fotoUrl)
        {
            if (Status != "em andamento")
                throw new Exception("Só é possível entregar uma atividade em andamento.");
            FotoUrl = fotoUrl;
            DataHoraEntrega = DateTime.Now;
            Status = "entregue";
        }

        // ganhosNaSemana: minutos já aprovados para o cliente na semana da entrega desta atividade
        public void Avaliar(Funcionario avaliador, bool aprovado, string observacao, int ganhosNaSemana)
        {
            if (Status != "entregue")
                throw new Exception("Só é possível avaliar uma atividade entregue.");
            Avaliador = avaliador;
            Observacao = observacao;
            Status = aprovado ? "aprovado" : "recusado";

            // Aprovada perto do limite, concede só o que falta para chegar nele
            int restanteNaSemana = Math.Max(0, LimiteSemanalMinutos - ganhosNaSemana);
            MinutosConcedidos = aprovado ? Math.Min(MinutosRecompensa(), restanteNaSemana) : 0;
        }
    }
}
