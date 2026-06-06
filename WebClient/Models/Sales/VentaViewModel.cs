
using Domain.DTOs.Configuration;
using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class VentaViewModel : MainViewModel
{
    public VentaDTO Venta { get; set; } = new();
    public VendedorDTO? Vendedor { get; set; }
    public List<MesaDTO> Mesas { get; set; } = [];
    public PuntoVentaViewModel PuntoVenta { get; set; } = new();

    public VentaViewModel() : base() { }
}
