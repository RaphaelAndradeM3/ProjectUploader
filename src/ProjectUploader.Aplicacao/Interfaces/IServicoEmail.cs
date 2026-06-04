using System.Threading.Tasks;

namespace ProjectUploader.Aplicacao.Interfaces;

/// <summary>
/// Serviço de infraestrutura para envio de e-mails do sistema.
/// </summary>
public interface IServicoEmail
{
    /// <summary>
    /// Envia um e-mail com instruções para recuperação de acesso.
    /// </summary>
    Task EnviarEmailRecuperacaoAsync(string emailDestino, string linkOuCodigoRecuperacao);
}
