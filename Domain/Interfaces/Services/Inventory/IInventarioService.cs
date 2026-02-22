using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface IInventarioService
{
    Task<InventarioDTO> GetByIdProducto(long idProducto);
    Task<ResponseFilterDTO<InventarioDTO>> GetAll(FilterDTO? filter);
}

