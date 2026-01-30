using Domain.DTOs.Security;

namespace WebClient.Models.Security;

public class PerfilViewModel : MainViewModel
{
    public List<ModuloDTO> ListaModulos { get; set; }
    public PerfilDTO Perfil { get; set; }

    public PerfilViewModel() : base() { }
}

