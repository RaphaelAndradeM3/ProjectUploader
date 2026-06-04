using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Dominio.Repositorios;

/// <summary>
/// Interface de repositório para acesso a dados de Usuário.
/// </summary>
public interface IRepositorioUsuario
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ObterTodosAsync();
    Task AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
}
