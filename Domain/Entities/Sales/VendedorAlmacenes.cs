using Domain.Entities.Inventory;

namespace Domain.Entities.Sales;

public class VendedorAlmacenes : Entity
{
    public long IdVendedor { get; set; }
    public long IdAlmacen { get; set; }

    public Vendedor? Vendedor { get; set; }
    public Almacen? Almacen { get; set; }
}
