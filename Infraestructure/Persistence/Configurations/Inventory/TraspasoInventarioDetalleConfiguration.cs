using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class TraspasoInventarioDetalleConfiguration : IEntityTypeConfiguration<TraspasoInventarioDetalle>
{
    public void Configure(EntityTypeBuilder<TraspasoInventarioDetalle> builder)
    {
        builder.ToTable("TraspasoInventarioDetalle", "Inventario");

        builder.HasKey(detalle => detalle.Id);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdProducto);

        builder.HasOne(detalle => detalle.Lote)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdLote);
    }
}
