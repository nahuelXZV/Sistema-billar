using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class OrdenVentaConfiguration : IEntityTypeConfiguration<OrdenVenta>
{
    public void Configure(EntityTypeBuilder<OrdenVenta> builder)
    {
        builder.ToTable("OrdenVenta", "Venta");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SubTotalProductos)
            .HasPrecision(18, 2);

        builder.Property(a => a.SubTotalTiempo)
            .HasPrecision(18, 2);

        builder.Property(a => a.DescuentoGlobal)
            .HasPrecision(18, 2);

        builder.Property(a => a.RecargoGlobal)
            .HasPrecision(18, 2);

        builder.Property(a => a.Total)
            .HasPrecision(18, 2);

        builder.Property(a => a.TotalPagado)
            .HasPrecision(18, 2);

        builder.Property(a => a.SaldoPendiente)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.IdCliente);
    }
}
