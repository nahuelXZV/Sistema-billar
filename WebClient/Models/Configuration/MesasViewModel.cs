using Domain.DTOs.Configuration;

namespace WebClient.Models.Configuration;

public class MesasViewModel : MainViewModel
{
    public MesaDTO Mesa { get; set; }
    public List<TipoMesaDTO> ListaTiposMesa { get; set; }

    public MesasViewModel() : base() { }
}


