using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProjectUploader.API.Servicos;
using ProjectUploader.Aplicacao.CasosDeUso.Autenticacao;
using ProjectUploader.Aplicacao.CasosDeUso.Usuarios;
using ProjectUploader.Aplicacao.Interfaces;
using ProjectUploader.Dominio.Repositorios;
using ProjectUploader.Infraestrutura.Persistencia;
using ProjectUploader.Infraestrutura.Repositorios;
using ProjectUploader.Infraestrutura.Servicos;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Adicionar Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Fallback: Testar conexão com SQL Server (master) para ver se o serviço existe
bool useSqlite = false;
var connectionString = builder.Configuration.GetConnectionString("ConexaoPadrao") ?? "Server=(localdb)\\MSSQLLocalDB;Database=ProjectUploaderDB;Trusted_Connection=True;";

try
{
    var builderStr = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
    using var conn = new Microsoft.Data.SqlClient.SqlConnection(builderStr.ConnectionString);
    conn.Open();
}
catch
{
    useSqlite = true;
    Log.Warning("SQL Server não encontrado ou inacessível. Usando fallback para SQLite (ProjectUploader.db).");
}

// Configuração do Banco de Dados - Scoped
builder.Services.AddDbContext<ContextoBancoDados>(options =>
{
    if (useSqlite)
        options.UseSqlite("Data Source=ProjectUploader.db");
    else
        options.UseSqlServer(connectionString);
});

// Injeção de Dependência: Repositórios - Scoped
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioArquivo, RepositorioArquivo>();
builder.Services.AddScoped<IRepositorioLogEvento, RepositorioLogEvento>();

// Injeção de Dependência: Serviços de Infraestrutura - Singleton / Transient
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografiaHash>();
builder.Services.AddTransient<IServicoEmail, ProvedorEmailSmtp>();
builder.Services.AddSingleton<IServicoToken, ServicoToken>();

// Injeção de Dependência: Casos de Uso (Aplicação) - Transient
builder.Services.AddTransient<AutenticarUsuarioUseCase>();
builder.Services.AddTransient<CadastrarUsuarioUseCase>();
// IniciarUploadLoteUseCase e IniciarDownloadLoteUseCase executam do lado do Cliente WinForms (ClientApp), 
// A API atua como endpoint passivo recebendo o stream e validando.

// Configuração de Autenticação JWT
var chaveSecreta = builder.Configuration["Jwt:Key"] ?? "ProjectUploader_Super_Secret_Key_For_JWT_Min_32_Bytes_Long_String";
var key = Encoding.ASCII.GetBytes(chaveSecreta);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Garantir criação do banco e usuário Admin padrão
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ContextoBancoDados>();
    context.Database.EnsureCreated(); // Para dev; em prod usar Migrations

    var repoUsuario = scope.ServiceProvider.GetRequiredService<IRepositorioUsuario>();
    var cripto = scope.ServiceProvider.GetRequiredService<IServicoCriptografia>();
    
    if (repoUsuario.ObterPorNomeUsuarioAsync("Adm").Result == null)
    {
        var admin = new ProjectUploader.Dominio.Entidades.Usuario(
            System.Guid.NewGuid(), 
            "Administrador do Sistema", 
            "Adm", 
            "admin@projectuploader.com", 
            cripto.GerarHash("Adm1234"), 
            ProjectUploader.Dominio.Enums.TipoPerfil.Administrador);
        
        repoUsuario.AdicionarAsync(admin).Wait();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }