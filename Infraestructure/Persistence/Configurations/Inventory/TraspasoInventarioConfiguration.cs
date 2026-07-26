using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class TraspasoInventarioConfiguration : IEntityTypeConfiguration<TraspasoInventario>
{
    public void Configure(EntityTypeBuilder<TraspasoInventario> builder)
    {
        builder.ToTable("TraspasoInventario", "Inventario");

        builder.HasKey(traspaso => traspaso.Id);

        builder.Property(traspaso => traspaso.Glosa)
            .HasMaxLength(250);

        builder.HasOne(traspaso => traspaso.AlmacenOrigen)
            .WithMany()
            .HasForeignKey(traspaso => traspaso.IdAlmacenOrigen);

        builder.HasOne(traspaso => traspaso.AlmacenDestino)
            .WithMany()
            .HasForeignKey(traspaso => traspaso.IdAlmacenDestino);

        builder.HasOne(traspaso => traspaso.Usuario)
            .WithMany()
            .HasForeignKey(traspaso => traspaso.IdUsuario);

        builder.HasMany(traspaso => traspaso.Detalles)
            .WithOne(detalle => detalle.TraspasoInventario)
            .HasForeignKey(detalle => detalle.IdTraspasoInventario);
    }
}
