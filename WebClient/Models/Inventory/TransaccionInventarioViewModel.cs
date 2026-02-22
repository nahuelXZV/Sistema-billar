using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace WebClient.Models.Inventory;

public class TransaccionInventarioViewModel : MainViewModel
{
    public TransaccionInventarioDTO Transaccion { get; set; }
    public List<AlmacenDTO> ListadoAlmacen { get; set; }
    public List<ProductoDTO> ListadoProductos { get; set; }
    public List<SelectOptionDTO<short>> ListadoTipos { get; set; }

    public TransaccionInventarioViewModel() : base() { }
}
