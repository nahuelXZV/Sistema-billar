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

        builder.Property(a => a.Cantidad)
            .HasPrecision(18, 2);

        builder.Property(a => a.FactorConversion)
            .HasPrecision(18, 6);

        builder.Property(a => a.NombreUnidadMedida)
            .HasMaxLength(100);

        builder.Property(a => a.AbreviaturaUnidadMedida)
            .HasMaxLength(20);

        builder.Property(a => a.PrecioUnitario)
            .HasPrecision(18, 2);

        builder.Property(a => a.Descuento)
            .HasPrecision(18, 2);

        builder.Property(a => a.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(a => a.Total)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.Venta)
            .WithMany(a => a.ListaDetalles)
            .HasForeignKey(a => a.IdVenta);

        builder.HasOne(a => a.OrdenVentaDetalle)
            .WithMany()
            .HasForeignKey(a => a.IdOrdenVentaDetalle)
            .IsRequired(false);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.IdProducto);

        builder.HasOne(a => a.ProductoConversion)
            .WithMany()
            .HasForeignKey(a => a.IdProductoConversion)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
