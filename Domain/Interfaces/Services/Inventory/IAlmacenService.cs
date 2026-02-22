using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface IAlmacenService
{
    Task<long> Create(AlmacenDTO almacen);
    Task<bool> Update(AlmacenDTO almacen);
    Task<bool> Delete(long id);
    Task<AlmacenDTO> GetById(long id);
    Task<ResponseFilterDTO<AlmacenDTO>> GetAll(FilterDTO? filter);
    Task<List<AlmacenDTO>> GetAll();
}
