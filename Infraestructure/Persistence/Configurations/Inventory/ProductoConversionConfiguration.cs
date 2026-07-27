using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class ProductoConversionConfiguration : IEntityTypeConfiguration<ProductoConversion>
{
    public void Configure(EntityTypeBuilder<ProductoConversion> builder)
    {
        builder.ToTable("ProductoConversion", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.IdProducto);

        builder.HasOne(a => a.UnidadMedida)
            .WithMany()
            .HasForeignKey(a => a.IdUnidadMedida);
    }
}
