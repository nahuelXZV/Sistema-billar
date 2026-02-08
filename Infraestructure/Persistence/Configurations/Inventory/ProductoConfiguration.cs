using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Producto", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(c => c.IdCategoria);

        builder.HasOne<UnidadMedida>()
            .WithMany()
            .HasForeignKey(c => c.IdUnidadMedida);
    }
}