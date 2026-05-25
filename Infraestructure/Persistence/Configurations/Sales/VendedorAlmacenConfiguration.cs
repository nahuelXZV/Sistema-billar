using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class VendedorAlmacenConfiguration : IEntityTypeConfiguration<VendedorAlmacenes>
{
    public void Configure(EntityTypeBuilder<VendedorAlmacenes> builder)
    {
        builder.ToTable("VendedorAlmacen", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Vendedor)
            .WithMany(a => a.ListaAlmacenes)
            .HasForeignKey(a => a.IdVendedor);

        builder.HasOne(a => a.Almacen)
            .WithMany()
            .HasForeignKey(a => a.IdAlmacen);
    }
}
