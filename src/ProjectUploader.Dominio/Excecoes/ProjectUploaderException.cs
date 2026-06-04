using System;

namespace ProjectUploader.Dominio.Excecoes;

/// <summary>
/// Exceção base de domínio para o sistema ProjectUploader.
/// Todas as exceções de negócio devem herdar desta classe.
/// </summary>
public abstract class ProjectUploaderException : Exception
{
    protected ProjectUploaderException(string mensagem) : base(mensagem)
    {
    }

    protected ProjectUploaderException(string mensagem, Exception excecaoInterna) : base(mensagem, excecaoInterna)
    {
    }
}
