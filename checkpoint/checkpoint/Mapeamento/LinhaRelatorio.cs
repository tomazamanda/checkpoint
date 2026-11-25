namespace checkpoint.Mapeamento
{
    // Uma linha de relatório: serve para as consultas com GROUP BY
    public class LinhaRelatorio
    {
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public double Total { get; set; }

        public override string ToString()
        {
            return $"{Descricao,-30} {Quantidade,6}   R$ {Total,10:F2}";
        }
    }
}
