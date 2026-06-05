
using Domain.DTOs.Configuration;
using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class VentaViewModel : MainViewModel
{
    public VendedorDTO? Vendedor { get; set; }
    public PuntoVentaViewModel? PuntoVenta { get; set; }
    public List<MesaDTO> Mesas { get; set; } = [];

    public VentaViewModel() : base() { }
}
