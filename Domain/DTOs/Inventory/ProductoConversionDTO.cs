namespace Domain.DTOs.Inventory;

public class ProductoConversionDTO
{
    public long Id { get; set; }
    public long IdProducto { get; set; }
    public long IdUnidadMedida { get; set; }
    public decimal FactorConversion { get; set; }

    public ProductoDTO? Producto { get; set; }
    public UnidadMedidaDTO? UnidadMedida { get; set; }
}
