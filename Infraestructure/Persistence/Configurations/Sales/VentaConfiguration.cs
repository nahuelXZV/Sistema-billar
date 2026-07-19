using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Venta", "Venta");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.Property(a => a.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(a => a.Descuento)
            .HasPrecision(18, 2);

        builder.Property(a => a.Recargo)
            .HasPrecision(18, 2);

        builder.Property(a => a.Total)
            .HasPrecision(18, 2);

        builder.Property(a => a.TotalPagado)
            .HasPrecision(18, 2);

        builder.Property(a => a.Cambio)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.OrdenVenta)
            .WithMany()
            .HasForeignKey(a => a.IdOrdenVenta)
            .IsRequired(false);

        builder.HasOne(a => a.TurnoCaja)
            .WithMany()
            .HasForeignKey(a => a.IdTurnoCaja)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.IdCliente);

        builder.HasOne(a => a.Vendedor)
            .WithMany()
            .HasForeignKey(a => a.IdVendedor);

        builder.HasMany(a => a.ListaDetalles)
            .WithOne(a => a.Venta)
            .HasForeignKey(a => a.IdVenta);

        builder.HasMany(a => a.ListaPagos)
            .WithOne(a => a.Venta)
            .HasForeignKey(a => a.IdVenta);
    }
}
