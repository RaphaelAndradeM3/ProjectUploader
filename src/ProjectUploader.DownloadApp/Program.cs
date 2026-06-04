using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using ProjectUploader.DownloadApp.Formularios;
using ProjectUploader.DownloadApp.Servicos;

namespace ProjectUploader.DownloadApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        
        services.AddHttpClient<ApiClientDownloader>();
        services.AddTransient<FrmLogin>();
        services.AddTransient<FrmPrincipalDownload>();

        using var serviceProvider = services.BuildServiceProvider();

        var frmLogin = serviceProvider.GetRequiredService<FrmLogin>();
        Application.Run(frmLogin);
    }
}