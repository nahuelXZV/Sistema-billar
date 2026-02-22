using Domain.DTOs.Inventory;

namespace WebClient.Models.Inventory;

public class ListaPreciosViewModel : MainViewModel
{
    public ListaPrecioDTO ListaPrecio { get; set; }
    public List<ProductoDTO> ListaProductos { get; set; }
    public ListaPreciosViewModel() : base() { }
}
