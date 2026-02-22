namespace Domain.DTOs.Inventory;

public class ListaPrecioDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public List<ListaPrecioDetalleDTO>? ListaDetalles { get; set; }
}
