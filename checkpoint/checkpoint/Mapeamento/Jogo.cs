namespace checkpoint.Mapeamento
{
    public class Jogo
    {
        public int JogoId { get; set; }
        public string Nome { get; set; }
        public string Genero { get; set; }

        private int _classificacaoIndicativa;
        public int ClassificacaoIndicativa
        {
            get { return _classificacaoIndicativa; }
            set
            {
                if (value != 0 && value != 10 && value != 12 && value != 14 && value != 16 && value != 18)
                    throw new Exception("Classificação indicativa inválida (use 0, 10, 12, 14, 16 ou 18).");
                _classificacaoIndicativa = value;
            }
        }
    }
}
