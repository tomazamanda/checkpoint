namespace checkpoint.Mapeamento
{
    public class Obra
    {
        public int ObraId { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Editora { get; set; }
        public int FaixaEtaria { get; set; }
        public int? QtdPaginas { get; set; }
        public int QtdExemplares { get; set; }
        public int MinutosRecompensa { get; set; }

        private string _tipo;
        public string Tipo
        {
            get { return _tipo; }
            set
            {
                if (value != "livro" && value != "gibi" && value != "manga")
                    throw new Exception("Tipo de obra inválido (use livro, gibi ou manga).");
                _tipo = value;
            }
        }

        public override string ToString()
        {
            return $"[{ObraId}] {Titulo} ({Tipo}) - a partir de {FaixaEtaria} anos - vale {MinutosRecompensa} min";
        }
    }
}
