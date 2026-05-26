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
    public double SubTotalProductos { get; set; }
    public double SubTotalTiempo { get; set; }
    public double DescuentoGlobal { get; set; }
    public double RecargoGlobal { get; set; }
    public double Total { get; set; }
    public double TotalPagado { get; set; }
    public double SaldoPendiente { get; set; }
    public string? Observacion { get; set; }

    public ClienteDTO? Cliente { get; set; }
    public List<OrdenVentaDetalleDTO>? ListaDetalles { get; set; }
    public List<UsoMesaDTO>? ListaUsoMesas { get; set; }
}
