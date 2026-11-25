namespace checkpoint.Mapeamento
{
    public class Bairro
    {
        public int BairroId { get; set; }
        public string Nome { get; set; }
        public Cidade Cidade { get; set; }
    }
}