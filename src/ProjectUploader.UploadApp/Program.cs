using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using ProjectUploader.UploadApp.Formularios;
using ProjectUploader.UploadApp.Servicos;

namespace ProjectUploader.UploadApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

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
}