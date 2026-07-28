using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;

namespace WebClient.Models.Contact;

public class ProveedorViewModel : MainViewModel
{
    public ProveedorDTO Proveedor { get; set; }
    public List<ProductoDTO> ListaProductos { get; set; } = [];

    public ProveedorViewModel() : base()
    {
    }
}
