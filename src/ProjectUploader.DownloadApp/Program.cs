using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using ProjectUploader.DownloadApp.Formularios;
using ProjectUploader.DownloadApp.Servicos;
using Serilog;

namespace ProjectUploader.DownloadApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .WriteTo.File("Logs/log-download-.txt", rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        try
        {
            Serilog.Log.Information("Iniciando DownloadApp...");
            var services = new ServiceCollection();
        
        services.AddHttpClient<ApiClientDownloader>();
        services.AddTransient<FrmLogin>();
        services.AddTransient<FrmPrincipalDownload>();

        using var serviceProvider = services.BuildServiceProvider();

        var frmLogin = serviceProvider.GetRequiredService<FrmLogin>();
        Application.Run(frmLogin);
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Erro fatal no DownloadApp.");
        }
        finally
        {
            Serilog.Log.CloseAndFlush();
        }
    }
}