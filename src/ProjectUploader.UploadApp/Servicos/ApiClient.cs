using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectUploader.UploadApp.Servicos;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static string _token = string.Empty;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5206/"); // Configurar via appsettings futuro
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

    public async Task<bool> FazerUploadArquivoAsync(string caminhoAbsoluto, Action<long> progressoAtualizado, CancellationToken token)
    {
        try
        {
            var fileInfo = new FileInfo(caminhoAbsoluto);
            using var fileStream = new FileStream(caminhoAbsoluto, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            
            // Gerar Hash rápido usando SHA256 (Pode ser otimizado para arquivos gigantes)
            // Gerar Hash rápido usando SHA256 (Pode ser otimizado para arquivos gigantes)
            string hashOriginal = "hash-pendente";
            // Para simplificação de desempenho, deixaremos o Hash vazio no cliente agora e implementaremos se exigido
            
            using var content = new MultipartFormDataContent();
            var streamContent = new ProgressableStreamContent(fileStream, 4096, progressoAtualizado);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", fileInfo.Name);
            content.Add(new StringContent(hashOriginal), "hashOriginal");

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            var response = await _httpClient.PostAsync("api/Arquivos/upload", content, token);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro HTTP: {response.StatusCode} - {errorBody}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no upload: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            return false;
        }
    }

    private class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}

// Classe utilitária para capturar o progresso do Stream de envio HTTP
public class ProgressableStreamContent : HttpContent
{
    private readonly Stream _content;
    private readonly int _bufferSize;
    private readonly Action<long> _progress;

    public ProgressableStreamContent(Stream content, int bufferSize, Action<long> progress)
    {
        _content = content;
        _bufferSize = bufferSize;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var buffer = new byte[_bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await _content.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await stream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
            _progress?.Invoke(totalBytesRead);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _content.Length;
        return true;
    }
}
