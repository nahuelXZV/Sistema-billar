using Domain.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Purchases;

public class ProveedorProductoConfiguration : IEntityTypeConfiguration<ProveedorProducto>
{
    public void Configure(EntityTypeBuilder<ProveedorProducto> builder)
    {
        builder.ToTable("ProveedorProducto", "Compra");

        builder.HasKey(proveedorProducto => proveedorProducto.Id);

        builder.HasOne(proveedorProducto => proveedorProducto.Producto)
            .WithMany()
            .HasForeignKey(proveedorProducto => proveedorProducto.IdProducto);

        builder.HasOne(proveedorProducto => proveedorProducto.ProductoConversion)
            .WithMany()
            .HasForeignKey(proveedorProducto => proveedorProducto.IdProductoConversion);
    }
}
