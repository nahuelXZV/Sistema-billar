using Domain.Entities.Contact;

namespace Domain.Entities.Sales;

public class Venta : Entity
{
    public string Numero { get; set; } = string.Empty;
    public long? IdOrdenVenta { get; set; }
    public long IdCliente { get; set; }
    public long IdVendedor { get; set; }
    public DateTime Fecha { get; set; }
    public short Estado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal Cambio { get; set; }
    public decimal Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public OrdenVenta? OrdenVenta { get; set; }
    public Cliente? Cliente { get; set; }
    public Vendedor? Vendedor { get; set; }
}
