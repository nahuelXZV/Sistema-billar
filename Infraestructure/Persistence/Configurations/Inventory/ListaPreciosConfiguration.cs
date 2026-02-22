using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class ListaPreciosConfiguration : IEntityTypeConfiguration<ListaPrecios>
{
    public void Configure(EntityTypeBuilder<ListaPrecios> builder)
    {
        builder.ToTable("ListaPrecios", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasMany(c => c.ListaDetalles)
              .WithOne()
              .HasForeignKey(c => c.IdListaPrecio);
    }
}