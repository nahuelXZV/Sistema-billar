
using Domain.Entities.Inventory;

namespace Domain.DTOs.Inventory;

public class TransaccionInventarioDetalleDTO
{
    public long Id { get; set; }
    public double Cantidad { get; set; }
    public long IdTransaccion { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public long? IdLote { get; set; }
    public long IdAlmacen { get; set; }

    public string NombreProducto { get; set; }
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public string CodigoLote { get; set; }
    public string NombreAlmacen { get; set; }

    public TransaccionInventario? TransaccionInventario { get; set; }
    public ProductoDTO? Producto { get; set; }
    public LoteDTO? Lote { get; set; }
    public AlmacenDTO? Almacen { get; set; }
}
