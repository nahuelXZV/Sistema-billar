using Domain.DTOs.Contact;

namespace Domain.DTOs.Sales;

public class OrdenVentaDTO
{
    public long Id { get; set; }
    public long? IdCliente { get; set; }
    public string Numero { get; set; } = string.Empty;
    public short Estado { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal SubTotalProductos { get; set; }
    public decimal SubTotalTiempo { get; set; }
    public decimal DescuentoGlobal { get; set; }
    public decimal RecargoGlobal { get; set; }
    public decimal Total { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? Observacion { get; set; }

    public ClienteDTO? Cliente { get; set; }
    public List<OrdenVentaDetalleDTO>? ListaDetalles { get; set; }
    public List<UsoMesaDTO>? ListaUsoMesas { get; set; }
}
