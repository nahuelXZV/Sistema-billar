using Domain.Entities.Purchases;

namespace Domain.Entities.Contact;

public class Proveedor : Entity
{
    public string? NombreComercial { get; set; }
    public string? NombreContacto { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    public List<ProveedorProducto> ListaProductos { get; set; } = [];
}
