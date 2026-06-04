
using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class VentaViewModel : MainViewModel
{
    public VendedorDTO? Vendedor { get; set; }
    public PuntoVentaViewModel? PuntoVenta { get; set; }

    public VentaViewModel() : base() { }
}
