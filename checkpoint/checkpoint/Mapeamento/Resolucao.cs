namespace checkpoint.Mapeamento
{
    public class Resolucao : AtividadeSocial
    {
        public int ResolucaoId { get; set; }
        public Desafio Desafio { get; set; }

        public override int MinutosRecompensa()
        {
            return Desafio.MinutosRecompensa;
        }

        public override string ToString()
        {
            return $"[{ResolucaoId}] {Cliente.Nome} - {Desafio.Titulo} - {Status}";
        }
    }
}
