using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface IUnidadMedidaService
{
    Task<long> Create(UnidadMedidaDTO unidad);
    Task<bool> Update(UnidadMedidaDTO unidad);
    Task<bool> Delete(long id);
    Task<UnidadMedidaDTO> GetById(long id);
    Task<ResponseFilterDTO<UnidadMedidaDTO>> GetAll(FilterDTO? filter);
    Task<List<UnidadMedidaDTO>> GetAll();
}
