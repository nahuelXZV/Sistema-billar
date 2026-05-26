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

        builder.HasOne(a => a.OrdenVenta)
            .WithMany()
            .HasForeignKey(a => a.IdOrdenVenta);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.IdProducto);

        builder.HasOne(a => a.UsoMesa)
            .WithMany()
            .HasForeignKey(a => a.IdUsoMesa);

        builder.HasOne(a => a.Vendedor)
            .WithMany()
            .HasForeignKey(a => a.IdVendedor);
    }
}
