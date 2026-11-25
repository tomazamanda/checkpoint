namespace checkpoint.Database
{
    /* Guarda a foto do resumo (escrito à mão) ou da resolução, tirada pelo funcionário na entrega, fica em wwwroot/fotos: só abre para quem está logado (Program.cs) e fica fora do Git.
       Pesquisei para estudar: IFormFile (o arquivo que veio do formulário), Stream, File e Path (ler e gravar arquivo no disco) e Guid (um código aleatório que não se repete, usado no nome da foto). */
    public static class Fotos
    {
        public const long TamanhoMaximo = 5 * 1024 * 1024;   // 5 MB

        // Devolve o caminho que vai para o banco, por exemplo "fotos/resumos/22_3f2a9c....jpg"
        public static string Salvar(IFormFile foto, string wwwroot, string pasta, int id)
        {
            if (foto == null || foto.Length == 0)
                throw new Exception("Escolha a foto do trabalho.");
            if (foto.Length > TamanhoMaximo)
                throw new Exception("A foto pode ter até 5 MB.");

            // Confere os primeiros bytes do arquivo, e não o nome: um arquivo qualquer renomeado para .jpg é recusado
            byte[] inicio = new byte[4];
            using (Stream leitura = foto.OpenReadStream())
                leitura.ReadAtLeast(inicio, inicio.Length, throwOnEndOfStream: false);

            string extensao;
            if (inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF)
                extensao = ".jpg";
            else if (inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47)
                extensao = ".png";
            else
                throw new Exception("A foto precisa ser JPG ou PNG.");

            // O nome é do sistema, nunca o do arquivo enviado: evita "../" no nome e fotos com o mesmo nome
            string caminho = $"fotos/{pasta}/{id}_{Guid.NewGuid():N}{extensao}";
            string completo = Path.Combine(wwwroot, caminho);
            Directory.CreateDirectory(Path.GetDirectoryName(completo));
            using (FileStream arquivo = File.Create(completo))
                foto.CopyTo(arquivo);
            return caminho;
        }

        public static void Apagar(string wwwroot, string caminho)
        {
            File.Delete(Path.Combine(wwwroot, caminho));
        }
    }
}
