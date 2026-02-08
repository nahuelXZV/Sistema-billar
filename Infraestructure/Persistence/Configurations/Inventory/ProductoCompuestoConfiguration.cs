using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class ProductoCompuestoConfiguration : IEntityTypeConfiguration<ProductoCompuesto>
{
    public void Configure(EntityTypeBuilder<ProductoCompuesto> builder)
    {
        builder.ToTable("ProductoCompuesto", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne<Producto>()
            .WithMany()
            .HasForeignKey(c => c.IdProductoComponente);
    }
}