using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Security;

namespace Infraestructure.Persistence.Configurations.Segurity;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfil", "Seguridad");

        builder.HasKey(a => a.Id);

        builder.HasMany(c => c.ListaAccesos)
              .WithOne()
              .HasForeignKey(c => c.PerfilId);

        //builder.HasOne(c => c.Modulo)
        //      .WithMany()
        //      .HasForeignKey(c => c.ModuloId);
    }
}

