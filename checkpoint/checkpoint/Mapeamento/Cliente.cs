namespace checkpoint.Mapeamento
{
    public class Cliente : Pessoa
    {
        public int ClienteId { get; set; }
        public DateOnly DataCadastro { get; set; }
        public string Parentesco { get; set; }
        public DateOnly? DataAutorizacao { get; set; }
        public Responsavel Responsavel { get; set; }   // null para cliente adulto

        public void ValidarResponsavel()
        {
            if (EhMenorDeIdade() && Responsavel == null)
                throw new Exception("Cliente menor de idade precisa de um responsável.");
        }

        public override string Apresentar()
        {
            string situacao = Responsavel == null
                ? "adulto"
                : $"responsável: {Responsavel.Nome} ({Parentesco})";
            return $"[{ClienteId}] {Nome} - {Idade()} anos - {situacao}";
        }
    }
}