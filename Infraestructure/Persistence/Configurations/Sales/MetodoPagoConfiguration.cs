using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodoPago", "Venta");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(m => m.Abreviatura)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.ClaveMoneda)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(m => m.Icono)
            .HasMaxLength(100);
    }
}
