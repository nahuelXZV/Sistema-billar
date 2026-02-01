using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categoria", "Inventario");

        builder.HasKey(a => a.Id);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(c => c.IdCategoriaPadre);
    }
}