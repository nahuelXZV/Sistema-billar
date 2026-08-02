using Domain.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Purchases;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compra", "Compra");

        builder.HasKey(compra => compra.Id);

        builder.HasOne(compra => compra.Proveedor)
            .WithMany()
            .HasForeignKey(compra => compra.IdProveedor);

        builder.HasOne(compra => compra.Almacen)
            .WithMany()
            .HasForeignKey(compra => compra.IdAlmacen);

        builder.HasOne(compra => compra.Usuario)
            .WithMany()
            .HasForeignKey(compra => compra.IdUsuario);

        builder.HasOne(compra => compra.UsuarioAnulacion)
            .WithMany()
            .HasForeignKey(compra => compra.IdUsuarioAnulacion);

        builder.HasOne(compra => compra.TransaccionInventario)
            .WithMany()
            .HasForeignKey(compra => compra.IdTransaccionInventario);

        builder.HasMany(compra => compra.ListaDetalles)
            .WithOne(detalle => detalle.Compra)
            .HasForeignKey(detalle => detalle.IdCompra);
    }
}
