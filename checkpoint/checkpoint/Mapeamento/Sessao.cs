namespace checkpoint.Mapeamento
{
    public class Sessao
    {
        public int SessaoId { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public int? Minutos { get; set; }
        public double? Valor { get; set; }
        public Cliente Cliente { get; set; }
        public Estacao Estacao { get; set; }
        public Comanda Comanda { get; set; }   // null na sessão por crédito social

        private string _origem;
        public string Origem
        {
            get { return _origem; }
            set
            {
                if (value != "paga" && value != "credito social")
                    throw new Exception("Origem inválida (use paga ou credito social).");
                _origem = value;
            }
        }

        public Sessao()
        {
            DataHoraInicio = DateTime.Now;
        }

        public void Validar()
        {
            if (Origem == "paga" && Comanda == null)
                throw new Exception("Sessão paga precisa estar ligada a uma comanda.");
            if (Origem == "paga" && Comanda.Status != "aberta")
                throw new Exception("A comanda informada não está aberta.");
            if (Origem == "paga" && (Comanda.Cliente == null || Comanda.Cliente.ClienteId != Cliente.ClienteId))
                throw new Exception($"A comanda {Comanda.ComandaId} não é deste cliente. Use uma comanda aberta no nome dele.");
            if (Origem == "credito social" && Comanda != null)
                throw new Exception("Sessão por crédito social não tem comanda.");
        }

        public void Encerrar()
        {
            if (DataHoraFim != null)
                throw new Exception("Esta sessão já foi encerrada.");

            DataHoraFim = DateTime.Now;
            TimeSpan duracao = DataHoraFim.Value - DataHoraInicio;
            Minutos = (int)Math.Ceiling(duracao.TotalMinutes);

            if (Origem == "paga")
                Valor = Estacao.CalcularValor(Minutos.Value);
            else
                Valor = 0;
        }

        public override string ToString()
        {
            string fim = DataHoraFim == null ? "em andamento" : $"{Minutos} min - R$ {Valor:F2}";
            return $"[{SessaoId}] {Cliente.Nome} - estação {Estacao.Numero} - início {DataHoraInicio:dd/MM HH:mm} - {Origem} - {fim}";
        }
    }
}
