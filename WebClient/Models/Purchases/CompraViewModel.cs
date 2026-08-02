using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;
using Domain.DTOs.Purchases;

namespace WebClient.Models.Purchases;

public class CompraViewModel : MainViewModel
{
    public CompraDTO Compra { get; set; } = new();
    public List<ProveedorDTO> ListaProveedores { get; set; } = [];
    public List<AlmacenDTO> ListaAlmacenes { get; set; } = [];
    public List<ProductoDTO> ListaProductos { get; set; } = [];
}
