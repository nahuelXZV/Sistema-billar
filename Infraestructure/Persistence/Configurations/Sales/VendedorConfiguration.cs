using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class VendedorConfiguration : IEntityTypeConfiguration<Vendedor>
{
    public void Configure(EntityTypeBuilder<Vendedor> builder)
    {
        builder.ToTable("Vendedor", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.IdUsuario)
            .IsRequired(false);

        builder.HasOne(a => a.ListaPrecio)
            .WithMany()
            .HasForeignKey(a => a.IdListaPrecio)
            .IsRequired(false);

        builder.HasMany(a => a.ListaAlmacenes)
            .WithOne(a => a.Vendedor)
            .HasForeignKey(a => a.IdVendedor);
    }
}
