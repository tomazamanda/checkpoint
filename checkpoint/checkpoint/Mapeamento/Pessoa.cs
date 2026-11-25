namespace checkpoint.Mapeamento
{
    public abstract class Pessoa
    {
        public int PessoaId { get; set; }
        public string Nome { get; set; }
        public DateOnly DataNasc { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public Endereco Endereco { get; set; }   // pode ser null: relação (0,1)

        private string _cpf;
        public string Cpf
        {
            get { return _cpf; }
            set
            {
                if (value == null)
                    throw new Exception("O CPF deve ter 11 números.");
                string numeros = value.Replace(".", "").Replace("-", "");
                if (numeros.Length != 11)
                    throw new Exception("O CPF deve ter 11 números.");
                foreach (char c in numeros)
                {
                    if (c < '0' || c > '9')
                        throw new Exception("O CPF deve ter 11 números.");
                }
                _cpf = numeros;
            }
        }

        public int Idade()
        {
            DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);
            int idade = hoje.Year - DataNasc.Year;
            if (DataNasc > hoje.AddYears(-idade))   // ainda não fez aniversário este ano
                idade--;
            return idade;
        }

        public bool EhMenorDeIdade()
        {
            return Idade() < 18;
        }

        public abstract string Apresentar();
    }
}