using Domain.DTOs.Inventory;

namespace WebClient.Models.Inventory;

public class UnidadMedidaViewModel : MainViewModel
{
    public List<UnidadMedidaDTO> ListaUnidades { get; set; }
    public UnidadMedidaDTO Unidad { get; set; }

    public UnidadMedidaViewModel() : base() { }
}

