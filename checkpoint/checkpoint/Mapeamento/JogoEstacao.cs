namespace checkpoint.Mapeamento
{
    public class JogoEstacao
    {
        public int JogoEstacaoId { get; set; }
        public DateOnly? DataInstalacao { get; set; }
        public Estacao Estacao { get; set; }
        public Jogo Jogo { get; set; }
    }
}
