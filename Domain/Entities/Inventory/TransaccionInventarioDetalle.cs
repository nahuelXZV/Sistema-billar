
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Inventory;

public class TransaccionInventarioDetalle : Entity
{
    public double Cantidad { get; set; }
    public long IdTransaccion { get; set; }
    public long IdProducto { get; set; }
    public long? IdLote { get; set; }
    public long IdAlmacen { get; set; }

    public TransaccionInventario? TransaccionInventario { get; set; }
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
    public Almacen? Almacen { get; set; }
}
