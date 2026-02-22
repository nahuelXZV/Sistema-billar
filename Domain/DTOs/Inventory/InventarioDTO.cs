namespace Domain.DTOs.Inventory;

public class InventarioDTO
{
    public long Id { get; set; }
    public double Cantidad { get; set; }
    public double Reservado { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public long IdProducto { get; set; }
    public long IdAlmacen { get; set; }
    public long? IdLote { get; set; }

    public ProductoDTO? Producto { get; set; }
    public LoteDTO? Lote { get; set; }
    public AlmacenDTO? Almacen { get; set; }
}
