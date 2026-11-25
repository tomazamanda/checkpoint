namespace checkpoint.Mapeamento
{
    public class Estacao
    {
        public int EstacaoId { get; set; }
        public int Numero { get; set; }
        public string Plataforma { get; set; }

        private string _tipo;
        public string Tipo
        {
            get { return _tipo; }
            set
            {
                if (value != "computador" && value != "videogame")
                    throw new Exception("Tipo de estação inválido (use computador ou videogame).");
                _tipo = value;
            }
        }

        private string _status = "disponivel";
        public string Status
        {
            get { return _status; }
            set
            {
                if (value != "disponivel" && value != "em uso" && value != "manutencao")
                    throw new Exception("Status de estação inválido.");
                _status = value;
            }
        }

        private double _precoHora;
        public double PrecoHora
        {
            get { return _precoHora; }
            set
            {
                if (value <= 0)
                    throw new Exception("O preço por hora deve ser maior que zero.");
                _precoHora = value;
            }
        }

        public double CalcularValor(int minutos)
        {
            return Math.Round(minutos / 60.0 * PrecoHora, 2);
        }

        public override string ToString()
        {
            return $"[{EstacaoId}] Estação {Numero} - {Tipo} ({Plataforma}) - R$ {PrecoHora:F2}/h - {Status}";
        }
    }
}
