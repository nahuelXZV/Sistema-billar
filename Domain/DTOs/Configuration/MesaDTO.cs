namespace Domain.DTOs.Configuration;

public class MesaDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public long IdTipoMesa { get; set; }
    public bool Activo { get; set; }
}
