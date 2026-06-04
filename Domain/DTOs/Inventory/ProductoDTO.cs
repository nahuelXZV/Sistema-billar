using Domain.Entities.Inventory;

namespace Domain.DTOs.Inventory;

public class ProductoDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string UrlImagen { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Marca { get; set; }
    public bool Activo { get; set; }
    public bool EsCompuesto { get; set; }
    public long IdCategoria { get; set; }
    public long IdUnidadMedida { get; set; }
    public short Tipo { get; set; }
    public Categoria? Categoria { get; set; }
    public UnidadMedida? UnidadMedida { get; set; }
    public List<ProductoCompuestoDTO>? ProductosCompuestos { get; set; }
    public List<LoteDTO>? ListadoLotes { get; set; }
}
