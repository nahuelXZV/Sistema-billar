using Domain.DTOs.Inventory;

namespace WebClient.Models.Inventory;

public class CategoriaViewModel : MainViewModel
{
    public List<CategoriaDTO> ListaCategorias { get; set; }
    public CategoriaDTO Categoria { get; set; }

    public CategoriaViewModel() : base() { }
}

