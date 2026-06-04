using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Infraestrutura.Persistencia.Mapeamentos;

public class MapeamentoLogEvento : IEntityTypeConfiguration<LogEvento>
{
    public void Configure(EntityTypeBuilder<LogEvento> builder)
    {
        builder.ToTable("LogsEventos");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Nivel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Operacao)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Detalhes)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.IpOrigem)
            .HasMaxLength(50);
        
        builder.HasIndex(l => l.DataHora);
    }
}
