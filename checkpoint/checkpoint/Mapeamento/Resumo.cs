namespace checkpoint.Mapeamento
{
    public class Resumo : AtividadeSocial
    {
        public int ResumoId { get; set; }
        public Obra Obra { get; set; }

        public override int MinutosRecompensa()
        {
            return Obra.MinutosRecompensa;
        }

        public override string ToString()
        {
            return $"[{ResumoId}] {Cliente.Nome} - {Obra.Titulo} - {Status}";
        }
    }
}
