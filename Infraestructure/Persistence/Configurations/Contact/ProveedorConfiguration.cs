using Domain.Entities.Contact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Persistence.Configurations.Contact;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedor", "Contacto");

        builder.HasKey(proveedor => proveedor.Id);

        builder.HasMany(proveedor => proveedor.ListaProductos)
            .WithOne(proveedorProducto => proveedorProducto.Proveedor)
            .HasForeignKey(proveedorProducto => proveedorProducto.IdProveedor);
    }
}
