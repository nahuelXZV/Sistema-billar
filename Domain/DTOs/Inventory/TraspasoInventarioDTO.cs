namespace Domain.DTOs.Inventory;

public class TraspasoInventarioDTO
{
    public long Id { get; set; }
    public long IdAlmacenOrigen { get; set; }
    public long IdAlmacenDestino { get; set; }
    public long IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public short Estado { get; set; }

    public AlmacenDTO? AlmacenOrigen { get; set; }
    public AlmacenDTO? AlmacenDestino { get; set; }
    public List<TraspasoInventarioDetalleDTO> Detalles { get; set; } = [];
}
