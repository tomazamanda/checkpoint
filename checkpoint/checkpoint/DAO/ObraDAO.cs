using checkpoint.Database;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using MySql.Data.MySqlClient;

namespace checkpoint.DAO
{
    public class ObraDAO : IDao<Obra>
    {
        public void Insert(Obra obra)
        {
            string sql = @"INSERT INTO obra (titulo, tipo, autor, editora, faixaEtaria, qtdPaginas, qtdExemplares, minutosRecompensa)
                VALUES (@titulo, @tipo, @autor, @editora, @faixaEtaria, @qtdPaginas, @qtdExemplares, @minutosRecompensa)";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, obra);
                cmd.ExecuteNonQuery();
                obra.ObraId = (int)cmd.LastInsertedId;
            }
        }

        public void Update(Obra obra)
        {
            string sql = @"UPDATE obra SET titulo = @titulo, tipo = @tipo, autor = @autor, editora = @editora,
                faixaEtaria = @faixaEtaria, qtdPaginas = @qtdPaginas, qtdExemplares = @qtdExemplares,
                minutosRecompensa = @minutosRecompensa WHERE obraId = @obraId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                Parametros(cmd, obra);
                cmd.Parameters.AddWithValue("@obraId", obra.ObraId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma obra foi atualizada (verifique o ID).");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM obra WHERE obraId = @obraId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@obraId", id);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Nenhuma obra encontrada com esse ID.");
            }
        }

        public List<Obra> List()
        {
            List<Obra> obras = new List<Obra>();
            string sql = "SELECT * FROM obra ORDER BY tipo, titulo";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    obras.Add(Mapear(dr));
            }
            return obras;
        }

        public Obra BuscarID(int id)
        {
            string sql = "SELECT * FROM obra WHERE obraId = @obraId";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@obraId", id);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Mapear(dr);
                    throw new Exception("Nenhuma obra encontrada com esse ID.");
                }
            }
        }

        private void Parametros(MySqlCommand cmd, Obra obra)
        {
            cmd.Parameters.AddWithValue("@titulo", obra.Titulo);
            cmd.Parameters.AddWithValue("@tipo", obra.Tipo);
            cmd.Parameters.AddWithValue("@autor", Converte.ParaBanco(obra.Autor));
            cmd.Parameters.AddWithValue("@editora", Converte.ParaBanco(obra.Editora));
            cmd.Parameters.AddWithValue("@faixaEtaria", obra.FaixaEtaria);
            cmd.Parameters.AddWithValue("@qtdPaginas", Converte.ParaBanco(obra.QtdPaginas));
            cmd.Parameters.AddWithValue("@qtdExemplares", obra.QtdExemplares);
            cmd.Parameters.AddWithValue("@minutosRecompensa", obra.MinutosRecompensa);
        }

        private Obra Mapear(MySqlDataReader dr)
        {
            Obra obra = new Obra();
            obra.ObraId = Convert.ToInt32(dr["obraId"]);
            obra.Titulo = dr["titulo"].ToString();
            obra.Tipo = dr["tipo"].ToString();
            obra.Autor = Converte.Texto(dr, "autor");
            obra.Editora = Converte.Texto(dr, "editora");
            obra.FaixaEtaria = Convert.ToInt32(dr["faixaEtaria"]);
            obra.QtdPaginas = Converte.Inteiro(dr, "qtdPaginas");
            obra.QtdExemplares = Convert.ToInt32(dr["qtdExemplares"]);
            obra.MinutosRecompensa = Convert.ToInt32(dr["minutosRecompensa"]);
            return obra;
        }
    }
}
