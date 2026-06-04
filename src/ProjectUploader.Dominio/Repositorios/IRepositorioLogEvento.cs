using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Dominio.Repositorios;

/// <summary>
/// Interface de repositório para persistência de Logs de Auditoria.
/// </summary>
public interface IRepositorioLogEvento
{
    Task AdicionarAsync(LogEvento logEvento);
}
