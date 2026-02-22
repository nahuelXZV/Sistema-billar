using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Inventory;

public class TransaccionInventarioConfiguration : IEntityTypeConfiguration<TransaccionInventario>
{
    public void Configure(EntityTypeBuilder<TransaccionInventario> builder)
    {
        builder.ToTable("TransaccionInventario", "Inventario");

        builder.HasKey(a => a.Id);
    }
}