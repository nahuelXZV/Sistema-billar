namespace Domain.DTOs.Sales;

public class OrdenMesaDetalleDTO
{
    public long Id { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public bool EsTiempoMesa { get; set; }
}
