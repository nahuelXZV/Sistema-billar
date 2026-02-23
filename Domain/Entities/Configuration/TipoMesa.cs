namespace Domain.Entities.Configuration;

public class TipoMesa : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public bool CobroPorTiempo { get; set; }
    public long? IdProducto { get; set; }
}
