namespace ProjectUploader.Aplicacao.Interfaces;

/// <summary>
/// Serviço de infraestrutura para geração e validação de hashes criptográficos (Senhas).
/// </summary>
public interface IServicoCriptografia
{
    /// <summary>
    /// Gera um hash seguro a partir de uma senha em texto plano.
    /// </summary>
    string GerarHash(string senhaTextoPlano);

    /// <summary>
    /// Valida se a senha em texto plano corresponde ao hash armazenado.
    /// </summary>
    bool ValidarSenha(string senhaTextoPlano, string hashArmazenado);
}
