using Domain.DTOs.Purchases;

namespace Domain.DTOs.Contact;

public class ProveedorDTO
{
    public long Id { get; set; }
    public string? NombreComercial { get; set; }
    public string? NombreContacto { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    public List<ProveedorProductoDTO> ListaProductos { get; set; } = [];
}
