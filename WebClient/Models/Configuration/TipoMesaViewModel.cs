using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;

namespace WebClient.Models.Configuration;

public class TipoMesaViewModel : MainViewModel
{
    public TipoMesaDTO TipoMesa { get; set; }
    public List<ProductoDTO> ListaProductos { get; set; }

}

