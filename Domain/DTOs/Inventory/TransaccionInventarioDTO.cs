
namespace Domain.DTOs.Inventory;

public class TransaccionInventarioDTO
{
    public long Id { get; set; }
    public short Tipo { get; set; }
    public DateTime? Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public long IdUsuario { get; set; }
    public long IdTransaccionInicial { get; set; }

    public List<TransaccionInventarioDetalleDTO> Detalles { get; set; }

}
