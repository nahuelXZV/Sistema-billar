using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface IListaPreciosService
{
    Task<long> Create(ListaPrecioDTO listaPrecio);
    Task<bool> Update(ListaPrecioDTO listaPrecio);
    Task<bool> Delete(long id);
    Task<ListaPrecioDTO> GetById(long id);
    Task<ResponseFilterDTO<ListaPrecioDTO>> GetAll(FilterDTO? filter);
    Task<List<ListaPrecioDTO>> GetAll();
}
