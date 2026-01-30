using Domain.DTOs.Inventory;

namespace WebClient.Models.Inventory;

public class AlmacenViewModel : MainViewModel
{
    public List<AlmacenDTO> ListaAlmacenes { get; set; }
    public AlmacenDTO Almacen { get; set; }

    public AlmacenViewModel() : base() { }
}

