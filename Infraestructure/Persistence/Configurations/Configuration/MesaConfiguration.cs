using Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Configuration;

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("Mesa", "Configuracion");

        builder.HasKey(a => a.Id);

        builder.HasOne(c => c.TipoMesa)
            .WithMany()
            .HasForeignKey(c => c.IdTipoMesa);
    }
}
