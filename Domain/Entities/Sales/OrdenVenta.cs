using Domain.Entities.Contact;

namespace Domain.Entities.Sales;

public class OrdenVenta : Entity
{
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

    public Cliente? Cliente { get; set; }
}
