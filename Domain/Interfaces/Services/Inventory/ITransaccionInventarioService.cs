using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface ITransaccionInventarioService
{
    Task<TransaccionInventarioDetalleDTO> GetByIdProducto(long idProducto);
    Task<ResponseFilterDTO<TransaccionInventarioDetalleDTO>> GetAll(FilterDTO? filter);
    Task<long> Create(TransaccionInventarioDTO transaccion);
    Task<bool> Update(TransaccionInventarioDTO transaccion);
}
