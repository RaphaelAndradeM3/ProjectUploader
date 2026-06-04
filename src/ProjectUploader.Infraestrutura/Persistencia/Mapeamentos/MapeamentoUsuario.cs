using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Infraestrutura.Persistencia.Mapeamentos;

public class MapeamentoUsuario : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NomeCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.NomeUsuario)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(u => u.NomeUsuario).IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Telefone).HasMaxLength(20);
        builder.Property(u => u.WhatsApp).HasMaxLength(20);
        builder.Property(u => u.Telegram).HasMaxLength(50);
        builder.Property(u => u.EmailExtra).HasMaxLength(150);
        builder.Property(u => u.DicaRecuperacao).HasMaxLength(100);

        builder.Property(u => u.RowVersion)
            .IsRowVersion();
    }
}
