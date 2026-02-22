using System.Security.Principal;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace WebClient.Models.Inventory;

public class ProductoViewModel : MainViewModel
{
    //public List<ProductoDTO>  { get; set; }
    public ProductoDTO Producto { get; set; }
    public List<CategoriaDTO> ListadoCategorias { get; set; }
    public List<UnidadMedidaDTO> ListadoUnidadesMedidas { get; set; }
    public List<ProductoDTO> ListaProductos { get; set; }
    public List<SelectOptionDTO<short>> ListaTipoProducto { get; set; }

    public ProductoViewModel() : base() { }
}

