namespace Domain.DTOs.Security;

public class PerfilDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }

    public List<PerfilAccesoDTO> ListaAccesos { get; set; }
}
