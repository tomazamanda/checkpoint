namespace checkpoint.Mapeamento
{
    public class Estado
    {
        public int EstadoId { get; set; }
        public string Nome { get; set; }

        private string _uf;
        public string Uf
        {
            get { return _uf; }
            set
            {
                if (value == null || value.Length != 2)
                    throw new Exception("A UF deve ter 2 letras.");
                _uf = value.ToUpper();
            }
        }
    }
}