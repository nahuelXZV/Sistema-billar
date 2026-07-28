namespace Domain.DTOs.Inventory;

public class ProductoPrecioVentaDTO
{
    public long IdProductoConversion { get; set; }
    public long IdUnidadMedida { get; set; }
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; }
    public decimal Precio { get; set; }
    public bool EsUnidadBase { get; set; }
}
