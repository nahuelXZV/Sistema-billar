using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class MetodoPagoViewModel : MainViewModel
{
    public List<MetodoPagoDTO> ListaMetodosPago { get; set; }
    public MetodoPagoDTO MetodoPago { get; set; }

    public MetodoPagoViewModel() : base() { }
}
