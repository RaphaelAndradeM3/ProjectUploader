namespace ProjectUploader.Dominio.Enums;

/// <summary>
/// Define os estados possíveis de um arquivo durante o processo de transferência.
/// </summary>
public enum StatusArquivo
{
    /// <summary>
    /// O arquivo está na fila, aguardando início.
    /// </summary>
    Pendente = 1,

    /// <summary>
    /// A transferência do arquivo está em andamento.
    /// </summary>
    EmAndamento = 2,

    /// <summary>
    /// A transferência foi concluída e validada com sucesso.
    /// </summary>
    Concluido = 3,

    /// <summary>
    /// Ocorreu um erro durante a leitura, envio ou validação.
    /// </summary>
    Erro = 4
}
