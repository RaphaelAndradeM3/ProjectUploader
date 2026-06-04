namespace ProjectUploader.Dominio.Enums;

/// <summary>
/// Define os perfis de acesso disponíveis no sistema.
/// </summary>
public enum TipoPerfil
{
    /// <summary>
    /// Usuário padrão que pode apenas enviar seus arquivos e ver seu histórico.
    /// </summary>
    Comum = 1,

    /// <summary>
    /// Administrador do sistema, com acesso total a usuários e logs.
    /// </summary>
    Administrador = 2
}
