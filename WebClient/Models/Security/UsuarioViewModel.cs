using Domain.DTOs.Security;

namespace WebClient.Models.Security;

public class UsuarioViewModel : MainViewModel
{
    public List<UsuarioDTO> ListaUsuarios { get; set; }
    public List<PerfilDTO> ListaPerfiles { get; set; }
    public UsuarioDTO Usuario { get; set; }

    public UsuarioViewModel() : base() { }
}

