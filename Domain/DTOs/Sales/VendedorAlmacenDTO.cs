using Domain.DTOs.Inventory;

namespace Domain.DTOs.Sales;

public class VendedorAlmacenDTO
{
    public long Id { get; set; }
    public long IdVendedor { get; set; }
    public long IdAlmacen { get; set; }

    public AlmacenDTO? Almacen { get; set; }
}
