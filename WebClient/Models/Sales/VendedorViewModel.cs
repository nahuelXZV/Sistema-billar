using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.DTOs.Security;

namespace WebClient.Models.Sales;

public class VendedorViewModel : MainViewModel
{
    public VendedorDTO Vendedor { get; set; }
    public List<UsuarioDTO> ListaUsuarios { get; set; } = new();
    public List<AlmacenDTO> ListaAlmacenes { get; set; } = new();
    public List<ListaPrecioDTO> ListaPrecios { get; set; } = new();

    public VendedorViewModel() : base() { }
}
