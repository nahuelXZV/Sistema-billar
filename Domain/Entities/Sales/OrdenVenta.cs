using Domain.Entities.Contact;

namespace Domain.Entities.Sales;

public class OrdenVenta : Entity
{
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

    public Cliente? Cliente { get; set; }
    public List<OrdenVentaDetalle> ListaDetalles { get; set; } = [];
    public List<UsoMesa> ListaUsoMesas { get; set; } = [];
}
