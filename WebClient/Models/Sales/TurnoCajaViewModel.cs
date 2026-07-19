using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class TurnoCajaViewModel : MainViewModel
{
    public TurnoCajaDTO TurnoCaja { get; set; } = new();
    public decimal MontoVendidoVendedor { get; set; }
    public bool EsCierreCaja { get; set; }
    public List<VendedorDTO> ListaVendedores { get; set; } = [];
    public List<MetodoPagoDTO> ListaMetodosPago { get; set; } = [];
}
