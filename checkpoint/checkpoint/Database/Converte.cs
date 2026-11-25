using MySql.Data.MySqlClient;

namespace checkpoint.Database
{
    /* Colunas que aceitam NULL: o MySQL devolve DBNull.Value, e o C# usa null, essa classe faz a tradução nos dois sentidos. 
       Pesquisei para estudar: DBNull.Value, que é o NULL do banco no C#. O parâmetro do MySqlCommand não aceita o null do C#. */
    public static class Converte
    {
        public static string Texto(MySqlDataReader dr, string coluna)
        {
            return dr[coluna] == DBNull.Value ? null : dr[coluna].ToString();
        }

        public static int? Inteiro(MySqlDataReader dr, string coluna)
        {
            return dr[coluna] == DBNull.Value ? null : Convert.ToInt32(dr[coluna]);
        }

        public static double? Numero(MySqlDataReader dr, string coluna)
        {
            return dr[coluna] == DBNull.Value ? null : Convert.ToDouble(dr[coluna]);
        }

        public static DateTime? DataHora(MySqlDataReader dr, string coluna)
        {
            return dr[coluna] == DBNull.Value ? null : Convert.ToDateTime(dr[coluna]);
        }

        public static DateOnly? Data(MySqlDataReader dr, string coluna)
        {
            return dr[coluna] == DBNull.Value ? null : DateOnly.FromDateTime(Convert.ToDateTime(dr[coluna]));
        }

        // C# -> MySQL: null vira DBNull.Value
        public static object ParaBanco(object valor)
        {
            if (valor == null)
                return DBNull.Value;
            return valor;
        }

        // DateOnly não é aceito direto pelo MySql.Data: converte para DateTime (hora 00:00)
        public static object ParaBanco(DateOnly? data)
        {
            return data.HasValue ? data.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
        }
    }
}