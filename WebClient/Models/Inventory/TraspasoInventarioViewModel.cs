using Domain.DTOs.Inventory;

namespace WebClient.Models.Inventory;

public class TraspasoInventarioViewModel : MainViewModel
{
    public TraspasoInventarioDTO Traspaso { get; set; } = new();
    public List<AlmacenDTO> ListadoAlmacenes { get; set; } = new();
}
