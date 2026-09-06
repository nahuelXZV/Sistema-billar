using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class OrdenVentaDetalleConfiguration : IEntityTypeConfiguration<OrdenVentaDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenVentaDetalle> builder)
    {
        builder.ToTable("OrdenVentaDetalle", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.IdCliente);

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

        builder.HasOne(a => a.OrdenVenta)
            .WithMany(a => a.ListaDetalles)
            .HasForeignKey(a => a.IdOrdenVenta);

        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.IdCliente)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.IdProducto);

        builder.HasOne(a => a.ProductoConversion)
            .WithMany()
            .HasForeignKey(a => a.IdProductoConversion)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(a => a.UsoMesa)
            .WithMany()
            .HasForeignKey(a => a.IdUsoMesa);

        builder.HasOne(a => a.Vendedor)
            .WithMany()
            .HasForeignKey(a => a.IdVendedor);
    }
}
