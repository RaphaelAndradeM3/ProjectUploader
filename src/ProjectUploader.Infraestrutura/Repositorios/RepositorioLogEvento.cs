using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.Infraestrutura.Repositorios;

public class RepositorioLogEvento : IRepositorioLogEvento
{
    private readonly Persistencia.ContextoBancoDados _contexto;

    public RepositorioLogEvento(Persistencia.ContextoBancoDados contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(LogEvento logEvento)
    {
        await _contexto.LogsEventos.AddAsync(logEvento);
        await _contexto.SaveChangesAsync();
    }
}
