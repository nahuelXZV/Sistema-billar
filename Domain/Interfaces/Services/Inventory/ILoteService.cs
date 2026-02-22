using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface ILoteService
{
    Task<long> Create(LoteDTO lote);
    Task<bool> Update(LoteDTO lote);
    Task<bool> Delete(long id);
    Task<LoteDTO> GetById(long id);
    Task<ResponseFilterDTO<LoteDTO>> GetAll(FilterDTO? filter);
    Task<List<LoteDTO>> GetAll();
}
