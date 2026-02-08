namespace Domain.DTOs.Inventory;

public class ProductoCompuestoDTO
{
    public long IdProductoPadre { get; set; }
    public long IdProductoComponente { get; set; }
    public double Cantidad { get; set; }

    public ProductoDTO? ProductoComponente { get; set; }
}
