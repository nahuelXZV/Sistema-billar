using Domain.DTOs.Contact;

namespace Domain.DTOs.Sales;

public class VentaDTO
{
    public long Id { get; set; }
    public Guid? IdempotencyKey { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long? IdOrdenVenta { get; set; }
    public long IdCliente { get; set; }
    public long IdVendedor { get; set; }
    public DateTime Fecha { get; set; }
    public short Estado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal Cambio { get; set; }
    public decimal Descuento { get; set; }
    public decimal Recargo { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public OrdenVentaDTO? OrdenVenta { get; set; }
    public ClienteDTO? Cliente { get; set; }
    public VendedorDTO? Vendedor { get; set; }
    public List<VentaDetalleDTO>? ListaDetalles { get; set; }
    public List<PagoVentaDTO>? ListaPagos { get; set; }
}
