using Domain.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Purchases;

public class CompraDetalleConfiguration : IEntityTypeConfiguration<CompraDetalle>
{
    public void Configure(EntityTypeBuilder<CompraDetalle> builder)
    {
        builder.ToTable("CompraDetalle", "Compra");

        builder.HasKey(detalle => detalle.Id);
      

        builder.HasOne(detalle => detalle.Compra)
            .WithMany(compra => compra.ListaDetalles)
            .HasForeignKey(detalle => detalle.IdCompra);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdProducto);

        builder.HasOne(detalle => detalle.ProductoConversion)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdProductoConversion);

        builder.HasOne(detalle => detalle.Lote)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdLote);
    }
}
