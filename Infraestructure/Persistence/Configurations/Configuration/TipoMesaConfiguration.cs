using Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Configuration;

public class TipoMesaConfiguration : IEntityTypeConfiguration<TipoMesa>
{
    public void Configure(EntityTypeBuilder<TipoMesa> builder)
    {
        builder.ToTable("TipoMesa", "Configuracion");

        builder.HasKey(a => a.Id);
    }
}
