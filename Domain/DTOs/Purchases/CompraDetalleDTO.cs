using Domain.DTOs.Inventory;

namespace Domain.DTOs.Purchases;

public class CompraDetalleDTO
{
    public long Id { get; set; }
    public long IdCompra { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public long? IdLote { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public decimal Cantidad { get; set; }
    public decimal CantidadBase { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoUnitarioBase { get; set; }
    public decimal Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }

    public ProductoDTO? Producto { get; set; }
    public ProductoConversionDTO? ProductoConversion { get; set; }
    public LoteDTO? Lote { get; set; }
}
