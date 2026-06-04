using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.Infraestrutura.Repositorios;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly Persistencia.ContextoBancoDados _contexto;

    public RepositorioUsuario(Persistencia.ContextoBancoDados contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _contexto.Usuarios.AddAsync(usuario);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _contexto.Usuarios.Update(usuario);
        await _contexto.SaveChangesAsync();
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario)
    {
        return await _contexto.Usuarios.FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario);
    }

    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
    {
        return await _contexto.Usuarios.ToListAsync();
    }
}
