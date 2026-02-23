
namespace Domain.DTOs.Configuration;

public class TipoMesaDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public bool CobroPorTiempo { get; set; }
    public long? IdProducto { get; set; }
}
