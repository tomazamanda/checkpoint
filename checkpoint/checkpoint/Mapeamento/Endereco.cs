namespace checkpoint.Mapeamento
{
    public class Endereco
    {
        public int EnderecoId { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Referencia { get; set; }
        public Bairro Bairro { get; set; }

        private string _cep;
        public string Cep
        {
            get { return _cep; }
            set
            {
                if (value == null)
                    throw new Exception("O CEP deve ter 8 números.");
                string numeros = value.Replace("-", "").Replace(".", "");
                if (numeros.Length != 8)
                    throw new Exception("O CEP deve ter 8 números.");
                foreach (char c in numeros)
                {
                    if (c < '0' || c > '9')
                        throw new Exception("O CEP deve ter 8 números.");
                }
                _cep = numeros;
            }
        }

        public override string ToString()
        {
            return $"{Logradouro}, {Numero} - {Bairro.Nome} - {Bairro.Cidade.Nome}/{Bairro.Cidade.Estado.Uf} - CEP {Cep}";
        }
    }
}