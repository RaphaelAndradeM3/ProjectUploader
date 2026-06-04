using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.Infraestrutura.Repositorios;

public class RepositorioArquivo : IRepositorioArquivo
{
    private readonly Persistencia.ContextoBancoDados _contexto;

    public RepositorioArquivo(Persistencia.ContextoBancoDados contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(ArquivoTransferencia arquivo)
    {
        await _contexto.Arquivos.AddAsync(arquivo);
        await _contexto.SaveChangesAsync();
    }

    public async Task AtualizarAsync(ArquivoTransferencia arquivo)
    {
        _contexto.Arquivos.Update(arquivo);
        await _contexto.SaveChangesAsync();
    }

    public async Task<ArquivoTransferencia?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Arquivos.FindAsync(id);
    }

    public async Task<IEnumerable<ArquivoTransferencia>> ObterPorUsuarioAsync(Guid idUsuario)
    {
        return await _contexto.Arquivos
            .Where(a => a.IdUsuario == idUsuario)
            .OrderByDescending(a => a.DataRegistro)
            .ToListAsync();
    }

    public async Task<IEnumerable<ArquivoTransferencia>> ObterTodosAsync()
    {
        return await _contexto.Arquivos
            .OrderByDescending(a => a.DataRegistro)
            .ToListAsync();
    }
}
