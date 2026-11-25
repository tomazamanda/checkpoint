namespace checkpoint.Mapeamento
{
    public class Produto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; }
        public string Unidade { get; set; }
        public double? PrecoVenda { get; set; }   // null para material interno
        public int QtdMinima { get; set; }

        private string _tipo;
        public string Tipo
        {
            get { return _tipo; }
            set
            {
                if (value != "venda" && value != "material interno")
                    throw new Exception("Tipo de produto inválido (use venda ou material interno).");
                _tipo = value;
            }
        }

        private int _qtdEstoque;
        public int QtdEstoque
        {
            get { return _qtdEstoque; }
            set
            {
                if (value < 0)
                    throw new Exception("O estoque não pode ficar negativo.");
                _qtdEstoque = value;
            }
        }

        public bool EstoqueBaixo()
        {
            return QtdEstoque <= QtdMinima;
        }

        // Produto à venda precisa de preço; material interno (papel, toner) não é vendido
        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new Exception("Informe o nome do produto.");
            if (Nome.Length > 100)
                throw new Exception("O nome do produto pode ter até 100 letras.");
            if (string.IsNullOrWhiteSpace(Unidade) || Unidade.Length > 10)
                throw new Exception("Informe a unidade com até 10 letras (un, pacote, porção...).");
            if (Tipo == "venda" && (PrecoVenda == null || PrecoVenda <= 0))
                throw new Exception("Produto à venda precisa de um preço maior que zero.");
            if (Tipo == "material interno")
                PrecoVenda = null;
            if (QtdMinima < 0)
                throw new Exception("O estoque mínimo não pode ser negativo.");
        }

        public override string ToString()
        {
            string preco = PrecoVenda == null ? "uso interno" : $"R$ {PrecoVenda:F2}";
            return $"[{ProdutoId}] {Nome} - {preco} - estoque: {QtdEstoque} {Unidade} (mínimo {QtdMinima})";
        }
    }
}
