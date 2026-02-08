using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface IProductoService
{
    Task<long> Create(ProductoDTO producto);
    Task<bool> Update(ProductoDTO producto);
    Task<bool> Delete(long id);
    Task<ProductoDTO> GetById(long id);
    Task<ResponseFilterDTO<ProductoDTO>> GetAll(FilterDTO? filter);
    Task<List<ProductoDTO>> GetAll();
}
