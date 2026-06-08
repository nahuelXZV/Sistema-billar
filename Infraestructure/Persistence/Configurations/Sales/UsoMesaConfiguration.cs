using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class UsoMesaConfiguration : IEntityTypeConfiguration<UsoMesa>
{
    public void Configure(EntityTypeBuilder<UsoMesa> builder)
    {
        builder.ToTable("UsoMesa", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.OrdenVenta)
            .WithMany(a => a.ListaUsoMesas)
            .HasForeignKey(a => a.IdOrdenVenta);

        builder.HasOne(a => a.Mesa)
            .WithMany()
            .HasForeignKey(a => a.IdMesa);
    }
}
