
namespace Domain.DTOs.Inventory;

public class ListaPrecioDetalleDTO
{
    public long IdListaPrecio { get; set; }
    public long IdProducto { get; set; }
    public double Precio { get; set; }

    public string NombreProducto { get; set; } = string.Empty;
}
