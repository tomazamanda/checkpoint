namespace checkpoint.Mapeamento
{
    public class Caixa
    {
        public int CaixaId { get; set; }
        public DateTime DataHoraAbertura { get; set; }
        // Pesquisei para estudar: o ? depois do tipo (DateTime?, int?, double?) deixa a propriedade aceitar null, aqui null quer dizer que o caixa ainda não fechou. O .Value pega o valor quando não é null.
        public DateTime? DataHoraFechamento { get; set; }
        public double? ValorFechamento { get; set; }
        public string Status { get; set; }
        public Funcionario Funcionario { get; set; }

        private double _valorAbertura;
        public double ValorAbertura
        {
            get { return _valorAbertura; }
            set
            {
                if (value < 0)
                    throw new Exception("O valor de abertura não pode ser negativo.");
                _valorAbertura = value;
            }
        }

        public Caixa()
        {
            DataHoraAbertura = DateTime.Now;
            Status = "aberto";
        }

        public void Fechar(double totalComandas)
        {
            if (Status == "fechado")
                throw new Exception("Este caixa já está fechado.");
            DataHoraFechamento = DateTime.Now;
            ValorFechamento = ValorAbertura + totalComandas;
            Status = "fechado";
        }

        public override string ToString()
        {
            return $"[{CaixaId}] Caixa aberto em {DataHoraAbertura:dd/MM/yyyy HH:mm} por {Funcionario.Nome} - R$ {ValorAbertura:F2} - {Status}";
        }
    }
}
