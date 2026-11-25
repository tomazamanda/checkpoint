namespace checkpoint.Mapeamento
{
    public class Responsavel : Pessoa
    {
        public int ResponsavelId { get; set; }

        public override string Apresentar()
        {
            return $"[{ResponsavelId}] {Nome} - Tel: {(Telefone == null ? "não informado" : Telefone)}";
        }
    }
}