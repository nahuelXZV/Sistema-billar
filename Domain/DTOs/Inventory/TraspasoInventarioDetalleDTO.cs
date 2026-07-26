namespace Domain.DTOs.Inventory;

public class TraspasoInventarioDetalleDTO
{
    public long Id { get; set; }
    public long IdTraspasoInventario { get; set; }
    public long IdProducto { get; set; }
    public long? IdLote { get; set; }
    public decimal Cantidad { get; set; }

    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoLote { get; set; } = string.Empty;

    public ProductoDTO? Producto { get; set; }
    public LoteDTO? Lote { get; set; }
}
