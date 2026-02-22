using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class ListaPreciosDetallesConfiguration : IEntityTypeConfiguration<ListaPreciosDetalle>
{
    public void Configure(EntityTypeBuilder<ListaPreciosDetalle> builder)
    {
        builder.ToTable("ListaPreciosDetalle", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Producto)
              .WithMany()
              .HasForeignKey(c => c.IdProducto);
    }
}