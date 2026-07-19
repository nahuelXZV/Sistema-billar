namespace Domain.DTOs.Sales;

public class TurnoCajaDTO
{
    public long Id { get; set; }
    public long IdVendedor { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public VendedorDTO? Vendedor { get; set; }
    public List<TurnoCajaDetalleDTO> Detalles { get; set; } = [];
}
