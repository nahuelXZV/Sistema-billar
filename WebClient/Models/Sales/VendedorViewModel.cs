using Domain.DTOs.Sales;
using Domain.DTOs.Security;

namespace WebClient.Models.Sales;

public class VendedorViewModel : MainViewModel
{
    public VendedorDTO Vendedor { get; set; }
    public List<UsuarioDTO> ListaUsuarios { get; set; }

    public VendedorViewModel() : base() { }
}
