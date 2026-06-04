using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace ProjectUploader.Testes;

[TestFixture]
public class FluxoCompletoE2ETests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _token;
    private string _nomeArquivo;

    [OneTimeSetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test, Order(1)]
    public async Task DeveAutenticarComoAdminERetornarToken()
    {
        // Act
        var response = await _client.PostAsJsonAsync("api/Auth/login", new { Login = "Adm", Senha = "Adm1234" });

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.That(authResponse, Is.Not.Null);
        Assert.That(authResponse!.Token, Is.Not.Empty);

        _token = authResponse.Token;
    }

    [Test, Order(2)]
    public async Task DeveFazerUploadDeArquivoComSucesso()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        var filePath = Path.GetTempFileName();
        _nomeArquivo = Path.GetFileName(filePath);
        await File.WriteAllTextAsync(filePath, "Teste de upload E2E via WebApplicationFactory.");
        
        try
        {
            using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            content.Add(streamContent, "file", _nomeArquivo);
            content.Add(new StringContent("hash-falso-e2e"), "hashOriginal");

            // Act
            var response = await _client.PostAsync("api/Arquivos/upload", content);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
    
    [Test, Order(3)]
    public async Task DeveListarArquivosComSucesso()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await _client.GetAsync("api/Arquivos");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var arquivos = await response.Content.ReadAsStringAsync();
        Assert.That(arquivos, Does.Contain(_nomeArquivo));
    }

    private class AuthResult
    {
        public string Token { get; set; } = string.Empty;
    }
}
