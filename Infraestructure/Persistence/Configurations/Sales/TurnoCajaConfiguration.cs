using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class TurnoCajaConfiguration : IEntityTypeConfiguration<TurnoCaja>
{
    public void Configure(EntityTypeBuilder<TurnoCaja> builder)
    {
        builder.ToTable("TurnoCaja", "Venta");

        builder.HasKey(turno => turno.Id);

        builder.Property(turno => turno.Observacion)
            .HasMaxLength(500);

        builder.HasIndex(turno => turno.IdVendedor)
            .IsUnique()
            .HasFilter("[Estado] = 1 AND [Eliminado] = 0");

        builder.HasOne(turno => turno.Vendedor)
            .WithMany()
            .HasForeignKey(turno => turno.IdVendedor)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(turno => turno.Detalles)
            .WithOne(detalle => detalle.TurnoCaja)
            .HasForeignKey(detalle => detalle.IdTurnoCaja)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
