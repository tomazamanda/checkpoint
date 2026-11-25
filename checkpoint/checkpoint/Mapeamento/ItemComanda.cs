namespace checkpoint.Mapeamento
{
    public class ItemComanda
    {
        public int ItemComandaId { get; set; }
        public double PrecoUnitario { get; set; }   // preço no momento da venda
        public Comanda Comanda { get; set; }
        public Produto Produto { get; set; }

        private int _quantidade;
        public int Quantidade
        {
            get { return _quantidade; }
            set
            {
                if (value <= 0)
                    throw new Exception("A quantidade deve ser maior que zero");
                _quantidade = value;
            }
        }

        public double Subtotal()
        {
            return PrecoUnitario * Quantidade;
        }

        public override string ToString()
        {
            return $"   {Quantidade}x {Produto.Nome} - R$ {PrecoUnitario:F2} = R$ {Subtotal():F2}";
        }
    }
}
