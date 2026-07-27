
namespace Domain.DTOs.Inventory;

public class ListaPrecioDetalleDTO
{
    public long Id { get; set; }
    public long IdListaPrecio { get; set; }
    public long IdProductoConversion { get; set; }
    public decimal Precio { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; }
    public ProductoConversionDTO? ProductoConversion { get; set; }
}
