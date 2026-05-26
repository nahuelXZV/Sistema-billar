using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class OrdenVentaConfiguration : IEntityTypeConfiguration<OrdenVenta>
{
    public void Configure(EntityTypeBuilder<OrdenVenta> builder)
    {
        builder.ToTable("OrdenVenta", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.IdCliente);
    }
}
