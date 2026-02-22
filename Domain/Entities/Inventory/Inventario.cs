
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Inventory;

public class Inventario : Entity
{
    public double Cantidad { get; set; }
    public double Reservado { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public long IdProducto { get; set; }
    public long IdAlmacen { get; set; }
    public long? IdLote { get; set; }
    
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
    public Almacen? Almacen { get; set; }
}