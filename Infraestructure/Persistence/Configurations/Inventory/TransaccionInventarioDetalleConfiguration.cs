using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class TransaccionInventarioDetalleConfiguration : IEntityTypeConfiguration<TransaccionInventarioDetalle>
{
    public void Configure(EntityTypeBuilder<TransaccionInventarioDetalle> builder)
    {
        builder.ToTable("TransaccionInventarioDetalle", "Inventario");

        builder.HasKey(a => a.Id);


        builder.HasOne(c => c.TransaccionInventario)
              .WithMany()
              .HasForeignKey(c => c.IdTransaccion);

        builder.HasOne(c => c.Lote)
              .WithMany()
              .HasForeignKey(c => c.IdLote);

        builder.HasOne(c => c.Almacen)
              .WithMany()
              .HasForeignKey(c => c.IdAlmacen);

        builder.HasOne(c => c.Producto)
              .WithMany()
              .HasForeignKey(c => c.IdProducto);
    }
}