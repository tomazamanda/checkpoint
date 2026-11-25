using checkpoint.DAO;
using checkpoint.Interface;
using checkpoint.Mapeamento;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using System.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Injeção de dependência: quando um controller pede a interface no construtor,
// a API entrega o DAO correspondente.
// Pesquisei para estudar: AddScoped (o slide de API só mostra o DbContext). Ele diz qual classe entregar
// para cada interface, e cria uma nova a cada pedido.
builder.Services.AddScoped<IClienteDAO, ClienteDAO>();
builder.Services.AddScoped<IDao<Responsavel>, ResponsavelDAO>();
builder.Services.AddScoped<IDao<Funcionario>, FuncionarioDAO>();
builder.Services.AddScoped<IFuncionarioDAO, FuncionarioDAO>();
builder.Services.AddScoped<IEstacaoDAO, EstacaoDAO>();
builder.Services.AddScoped<ISessaoDAO, SessaoDAO>();
builder.Services.AddScoped<ICaixaDAO, CaixaDAO>();
builder.Services.AddScoped<IComandaDAO, ComandaDAO>();
builder.Services.AddScoped<IItemComandaDAO, ItemComandaDAO>();
builder.Services.AddScoped<IProdutoDAO, ProdutoDAO>();
builder.Services.AddScoped<IDao<Obra>, ObraDAO>();
builder.Services.AddScoped<IDao<Desafio>, DesafioDAO>();
builder.Services.AddScoped<IAtividadeSocialDAO<Resumo>, ResumoDAO>();
builder.Services.AddScoped<IAtividadeSocialDAO<Resolucao>, ResolucaoDAO>();
builder.Services.AddScoped<RelatorioDAO>();

// Login por cookie: depois de entrar, o navegador manda o cookie em todo pedido.
// Pesquisei para estudar: AddAuthentication e AddCookie, que configuram esse login.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/login.html";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(10);   // um turno de trabalho
        opcoes.SlidingExpiration = true;

        // Telas sem login vão para a página de login. A API responde 401 com mensagem,
        // porque o fetch do front não segue redirecionamento para uma página HTML.
        opcoes.Events.OnRedirectToLogin = contexto =>
        {
            if (contexto.Request.Path.StartsWithSegments("/api"))
                return ResponderErro(contexto.HttpContext, 401, "Entre no sistema para continuar.");
            contexto.Response.Redirect(contexto.RedirectUri);
            return Task.CompletedTask;
        };
    });

// Tudo exige login, menos o que tiver [AllowAnonymous] e os arquivos públicos abaixo.
builder.Services.AddAuthorization(opcoes =>
{
    opcoes.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

WebApplication app = builder.Build();

// Mesmo papel do try/catch do menu de console: transforma o erro numa resposta
// com a mensagem, que o front mostra na tela.
// Pesquisei para estudar: middleware (app.Use). Todo pedido passa por aqui, e o await proximo() segue até o controller.
app.Use(async (contexto, proximo) =>
{
    try
    {
        await proximo();
    }
    catch (MySqlException ex)
    {
        // ex.Number é o código do erro do MySQL
        if (ex.Number == 1062)
            await ResponderErro(contexto, 409, "Já existe um registro com esses dados (valor repetido).");
        else if (ex.Number == 1451)
            await ResponderErro(contexto, 409, "Este registro é usado em outra tabela e não pode ser apagado.");
        else if (ex.Number == 1452)
            await ResponderErro(contexto, 400, "O ID informado não existe na tabela relacionada.");
        else
            await ResponderErro(contexto, 500, "Erro no banco de dados: " + ex.Message);
    }
    catch (Exception ex)
    {
        await ResponderErro(contexto, 400, ex.Message);
    }
});

// "no-cache": o navegador pode guardar HTML, CSS e JS, mas confere com o servidor antes de usar.
// Sem isso, depois de mudar uma tela, o Chrome continua mostrando a versão antiga.
StaticFileOptions arquivos = new StaticFileOptions();
arquivos.OnPrepareResponse = contexto => contexto.Context.Response.Headers.CacheControl = "no-cache";

app.UseDefaultFiles();   // "/" abre o wwwroot/index.html

// Arquivos que a tela de login precisa são entregues antes da checagem de login.
// Pesquisei para estudar: array já com os valores entre { }, UseWhen e Any. O Any confere se o endereço
// pedido começa com algum item da lista.
string[] publicos = { "/login.html", "/js/login.js", "/js/api.js", "/css", "/fontes", "/favicon.svg" };
app.UseWhen(
    contexto => publicos.Any(caminho => contexto.Request.Path.StartsWithSegments(caminho)),
    ramo => ramo.UseStaticFiles(arquivos));

app.UseAuthentication();   // lê o cookie e descobre quem está logado
app.UseAuthorization();    // barra quem não está logado

app.UseStaticFiles(arquivos);    // entrega os demais arquivos da pasta wwwroot (HTML, CSS, JS), só com login
app.MapControllers();    // liga as rotas /api/... aos controllers

// No F5 abre o navegador na página de início (o Visual Studio não usa o launchBrowser deste projeto).
// Pesquisei para estudar: Process.Start, que abre o endereço no navegador padrão do Windows.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        ProcessStartInfo navegador = new ProcessStartInfo(app.Urls.First());
        navegador.UseShellExecute = true;
        Process.Start(navegador);
    });
}

app.Run();

// Escreve a resposta { mensagem } que o front mostra na tela.
// Pesquisei para estudar: função escrita solta no Program.cs, fora de uma classe.
static async Task ResponderErro(HttpContext contexto, int status, string mensagem)
{
    contexto.Response.StatusCode = status;
    await contexto.Response.WriteAsJsonAsync(new { mensagem });
}
