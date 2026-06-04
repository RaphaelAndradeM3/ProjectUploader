using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Dominio.Repositorios;

/// <summary>
/// Interface de repositório para acesso a dados de ArquivoTransferencia.
/// </summary>
public interface IRepositorioArquivo
{
    Task<ArquivoTransferencia?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<ArquivoTransferencia>> ObterPorUsuarioAsync(Guid idUsuario);
    Task<IEnumerable<ArquivoTransferencia>> ObterTodosAsync();
    Task AdicionarAsync(ArquivoTransferencia arquivo);
    Task AtualizarAsync(ArquivoTransferencia arquivo);
}
