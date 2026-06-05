using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectUploader.DownloadApp.Servicos;

public class ApiClientDownloader
{
    private readonly HttpClient _httpClient;
    private static string _token = string.Empty;

    public ApiClientDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5206/"); // Idealmente buscaria de um configs
        
        if (!string.IsNullOrEmpty(_token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    public void DefinirToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<bool> AutenticarAsync(string login, string senha)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { Login = login, Senha = senha });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                DefinirToken(result.Token);
                return true;
            }
        }
        return false;
    }

    public async Task<List<ArquivoResponse>> ListarArquivosAsync()
    {
        var response = await _httpClient.GetAsync("api/Arquivos");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<ArquivoResponse>>() ?? new List<ArquivoResponse>();
        }
        return new List<ArquivoResponse>();
    }

    public async Task<bool> FazerDownloadArquivoAsync(Guid idArquivo, string pastaDestino, string nomeOriginal, Action<long> progressoAtualizado, CancellationToken token)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/Arquivos/download/{idArquivo}", HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var caminhoAbsoluto = Path.Combine(pastaDestino, nomeOriginal);

            using var contentStream = await response.Content.ReadAsStreamAsync(token);
            using var fileStream = new FileStream(caminhoAbsoluto, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            var buffer = new byte[4096];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                totalBytesRead += bytesRead;
                
                if (totalBytes > 0)
                {
                    var percentual = (totalBytesRead * 100) / totalBytes;
                    progressoAtualizado?.Invoke(percentual);
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ArquivoResponse
    {
        public Guid Id { get; set; }
        public string NomeOriginal { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public int Status { get; set; }
    }
}
