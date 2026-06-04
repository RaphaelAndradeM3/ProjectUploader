using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectUploader.Dominio.Entidades;

namespace ProjectUploader.Infraestrutura.Persistencia.Mapeamentos;

public class MapeamentoArquivoTransferencia : IEntityTypeConfiguration<ArquivoTransferencia>
{
    public void Configure(EntityTypeBuilder<ArquivoTransferencia> builder)
    {
        builder.ToTable("ArquivosTransferencia");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NomeOriginal)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.CaminhoServidor)
            .HasMaxLength(1000);

        builder.Property(a => a.HashVerificacao)
            .HasMaxLength(255);

        builder.Property(a => a.MensagemErro)
            .HasMaxLength(4000);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
