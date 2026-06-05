using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using ProjectUploader.UploadApp.Formularios;
using ProjectUploader.UploadApp.Servicos;
using Serilog;

namespace ProjectUploader.UploadApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .WriteTo.File("Logs/log-upload-.txt", rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        try
        {
            Serilog.Log.Information("Iniciando UploadApp...");
            var services = new ServiceCollection();
        
        // Registrar HttpClient para o ApiClient
        services.AddHttpClient<ApiClient>();
        
        // Registrar Formulários
        services.AddTransient<FrmLogin>();
        services.AddTransient<FrmPrincipalUpload>();

        using var serviceProvider = services.BuildServiceProvider();

        // Inicia pelo Form de Login
        var frmLogin = serviceProvider.GetRequiredService<FrmLogin>();
        Application.Run(frmLogin);
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Erro fatal no UploadApp.");
        }
        finally
        {
            Serilog.Log.CloseAndFlush();
        }
    }
}