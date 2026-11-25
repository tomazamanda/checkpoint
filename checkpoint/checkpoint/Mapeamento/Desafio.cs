namespace checkpoint.Mapeamento
{
    public class Desafio
    {
        public int DesafioId { get; set; }
        public string Titulo { get; set; }
        public string Enunciado { get; set; }
        public string Gabarito { get; set; }
        public int MinutosRecompensa { get; set; }

        private string _area;
        public string Area
        {
            get { return _area; }
            set
            {
                if (value != "logica" && value != "programacao")
                    throw new Exception("Área inválida (use logica ou programacao).");
                _area = value;
            }
        }

        private string _nivel;
        public string Nivel
        {
            get { return _nivel; }
            set
            {
                if (value != "facil" && value != "medio" && value != "dificil")
                    throw new Exception("Nível inválido (use facil, medio ou dificil).");
                _nivel = value;
            }
        }

        public override string ToString()
        {
            return $"[{DesafioId}] {Titulo} ({Area}, {Nivel}) - vale {MinutosRecompensa} min";
        }
    }
}
