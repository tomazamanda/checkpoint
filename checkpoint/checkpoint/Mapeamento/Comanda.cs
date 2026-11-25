namespace checkpoint.Mapeamento
{
    public class Comanda
    {
        public int ComandaId { get; set; }
        public DateTime DataHoraAbertura { get; set; }
        public DateTime? DataHoraFechamento { get; set; }
        public double? ValorTotal { get; set; }
        public Cliente Cliente { get; set; }   // null quando quem compra não tem cadastro
        public Caixa Caixa { get; set; }
        public List<ItemComanda> Itens { get; set; }
        public List<Sessao> Sessoes { get; set; }

        private string _formaPagamento;
        public string FormaPagamento
        {
            get { return _formaPagamento; }
            set
            {
                if (value != null && value != "dinheiro" && value != "pix" && value != "debito" && value != "credito")
                    throw new Exception("Forma de pagamento inválida.");
                _formaPagamento = value;
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value != "aberta" && value != "fechada" && value != "cancelada")
                    throw new Exception("Status de comanda inválido.");
                _status = value;
            }
        }

        public Comanda()
        {
            DataHoraAbertura = DateTime.Now;
            Status = "aberta";
            Itens = new List<ItemComanda>();
            Sessoes = new List<Sessao>();
        }

        public double CalcularValorTotal()
        {
            double total = 0;
            foreach (ItemComanda item in Itens)
                total += item.Subtotal();
            foreach (Sessao sessao in Sessoes)
            {
                if (sessao.Valor != null)   // sessão por crédito social não tem valor
                    total += sessao.Valor.Value;
            }

            ValorTotal = total;
            return total;
        }

        public void Fechar(string formaPagamento)
        {
            if (Status != "aberta")
                throw new Exception("Só é possível fechar uma comanda aberta.");
            if (Itens.Count == 0 && Sessoes.Count == 0)
                throw new Exception("A comanda está vazia. Use Cancelar comanda.");
            foreach (Sessao sessao in Sessoes)
            {
                if (sessao.DataHoraFim == null)
                    throw new Exception($"A sessão {sessao.SessaoId} ainda está em andamento. Encerre antes de fechar a comanda.");
            }

            FormaPagamento = formaPagamento;
            CalcularValorTotal();
            DataHoraFechamento = DateTime.Now;
            Status = "fechada";
        }

        // Cancelar = desistir da comanda sem cobrar. Só vale sem sessão: sessão jogada precisa ser paga, os produtos lançados voltam para o estoque (o controller apaga os itens).
        public void Cancelar()
        {
            if (Status != "aberta")
                throw new Exception("Só é possível cancelar uma comanda aberta.");
            if (Sessoes.Count > 0)
                throw new Exception("Esta comanda tem sessão. Encerre a sessão e feche a comanda.");

            ValorTotal = 0;
            DataHoraFechamento = DateTime.Now;
            Status = "cancelada";
        }

        public override string ToString()
        {
            string cliente = Cliente == null ? "sem cadastro" : Cliente.Nome;
            return $"[{ComandaId}] {cliente} - aberta em {DataHoraAbertura:dd/MM HH:mm} - {Status}";
        }
    }
}
