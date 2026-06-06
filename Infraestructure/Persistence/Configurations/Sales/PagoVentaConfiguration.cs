using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class PagoVentaConfiguration : IEntityTypeConfiguration<PagoVenta>
{
    public void Configure(EntityTypeBuilder<PagoVenta> builder)
    {
        builder.ToTable("PagoVenta", "Venta");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.MontoTotal)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.Venta)
            .WithMany(a => a.ListaPagos)
            .HasForeignKey(a => a.IdVenta);

        builder.HasOne(a => a.MetodoPago)
            .WithMany()
            .HasForeignKey(a => a.IdMetodoPago);
    }
}
