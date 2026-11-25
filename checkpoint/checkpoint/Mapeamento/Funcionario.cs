using checkpoint.Database;
using System.Text.Json.Serialization;

namespace checkpoint.Mapeamento
{
    public class Funcionario : Pessoa
    {
        public int FuncionarioId { get; set; }
        public string Cargo { get; set; }
        public DateOnly DataAdmissao { get; set; }

        // A lista de funcionários da API nunca mostra o hash da senha, pesquisei para estudar: [JsonIgnore], que deixa a propriedade fora do JSON.
        [JsonIgnore]
        public string SenhaHash { get; set; }

        public int AnosDeCasa()
        {
            TimeSpan tempo = DateTime.Today - DataAdmissao.ToDateTime(TimeOnly.MinValue);
            return tempo.Days / 365;
        }

        public bool ConferirSenha(string senha)
        {
            return Senha.Conferir(senha, SenhaHash);
        }

        public override string Apresentar()
        {
            return $"[{FuncionarioId}] {Nome} - {Cargo} - {AnosDeCasa()} ano(s) de casa";
        }
    }
}
