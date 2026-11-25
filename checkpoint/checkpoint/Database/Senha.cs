using System.Security.Cryptography;

namespace checkpoint.Database
{
    /* A senha nunca é gravada como texto. O banco guarda "iterações.sal.hash": o sal é aleatório para cada senha, então duas senhas iguais geram hashes diferentes, e o PBKDF2 repete o cálculo muitas vezes para deixar o teste de senhas em massa lento.
       Pesquisei para estudar: a parte de criptografia. Rfc2898DeriveBytes.Pbkdf2 calcula o hash, RandomNumberGenerator sorteia o sal, Convert.ToBase64String passa os bytes para texto (para caber no VARCHAR) e FixedTimeEquals compara os dois hashes. */
    public static class Senha
    {
        private const int Iteracoes = 100000;
        private const int TamanhoSal = 16;
        private const int TamanhoHash = 32;

        public static string GerarHash(string senha)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanhoSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
            return $"{Iteracoes}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Conferir(string senha, string senhaHash)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(senhaHash))
                return false;

            string[] partes = senhaHash.Split('.');
            if (partes.Length != 3)
                return false;

            int iteracoes = int.Parse(partes[0]);
            byte[] sal = Convert.FromBase64String(partes[1]);
            byte[] esperado = Convert.FromBase64String(partes[2]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            // compara sempre no mesmo tempo, para o tempo de resposta não dar pista da senha
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
