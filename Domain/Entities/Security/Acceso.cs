namespace Domain.Entities.Security;

public class Acceso : Entity
{
    public string Nombre { get; set; }
    public int Secuencia { get; set; }
    public string Controlador { get; set; }
    public string Vista { get; set; }
    public string Url { get; set; }
    public string Icono { get; set; }
    public string Descripcion { get; set; }
    public long ModuloId { get; set; }

    public Modulo Modulo { get; set; }
}
