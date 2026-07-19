using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Sales;

public class TurnoCajaDetalleConfiguration : IEntityTypeConfiguration<TurnoCajaDetalle>
{
    public void Configure(EntityTypeBuilder<TurnoCajaDetalle> builder)
    {
        builder.ToTable("TurnoCajaDetalle", "Venta");

        builder.HasKey(detalle => detalle.Id);

        builder.Property(detalle => detalle.MontoApertura)
            .HasPrecision(18, 2);

        builder.Property(detalle => detalle.MontoVentasSistema)
            .HasPrecision(18, 2);

        builder.Property(detalle => detalle.MontoCierreDeclarado)
            .HasPrecision(18, 2);

        builder.Property(detalle => detalle.Diferencia)
            .HasPrecision(18, 2);

        builder.HasIndex(detalle => new { detalle.IdTurnoCaja, detalle.IdMetodoPago })
            .IsUnique()
            .HasFilter("[Eliminado] = 0");

        builder.HasOne(detalle => detalle.MetodoPago)
            .WithMany()
            .HasForeignKey(detalle => detalle.IdMetodoPago)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
