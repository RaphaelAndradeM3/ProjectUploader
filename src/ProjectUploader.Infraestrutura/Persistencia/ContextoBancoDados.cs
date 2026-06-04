using Microsoft.EntityFrameworkCore;
using ProjectUploader.Dominio.Entidades;
using System.Reflection;

namespace ProjectUploader.Infraestrutura.Persistencia;

public class ContextoBancoDados : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ArquivoTransferencia> Arquivos => Set<ArquivoTransferencia>();
    public DbSet<LogEvento> LogsEventos => Set<LogEvento>();

    public ContextoBancoDados(DbContextOptions<ContextoBancoDados> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
