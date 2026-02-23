namespace Domain.Entities.Configuration;

public class Mesa : Entity
{
    public string Nombre { get; set; }
    public long IdTipoMesa { get; set; }
    public bool Activo { get; set; }

    public TipoMesa? TipoMesa { get; set; }
}
