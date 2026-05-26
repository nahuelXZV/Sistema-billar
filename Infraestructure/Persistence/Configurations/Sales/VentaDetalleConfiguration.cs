using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalle", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Venta)
            .WithMany()
            .HasForeignKey(a => a.IdVenta);

        builder.HasOne(a => a.OrdenVentaDetalle)
            .WithMany()
            .HasForeignKey(a => a.IdOrdenVentaDetalle)
            .IsRequired(false);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.IdProducto);
    }
}
