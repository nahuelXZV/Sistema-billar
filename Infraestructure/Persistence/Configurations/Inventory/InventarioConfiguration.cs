using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class InventarioConfiguration : IEntityTypeConfiguration<Inventario>
{
    public void Configure(EntityTypeBuilder<Inventario> builder)
    {
        builder.ToTable("Inventario", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne(c => c.Lote)
              .WithMany()
              .HasForeignKey(c => c.IdLote);

        builder.HasOne(a => a.Almacen)
          .WithMany()
          .HasForeignKey(c => c.IdAlmacen);

        builder.HasOne(c => c.Producto)
          .WithMany()
          .HasForeignKey(c => c.IdProducto);
    }
}