using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface ITurnoCajaService
{
    Task<long> Create(TurnoCajaDTO turnoCaja);
    Task<bool> Update(TurnoCajaDTO turnoCaja);
    Task<bool> Delete(long id);
    Task<bool> TieneActivo(long idVendedor);
    Task<TurnoCajaDTO> GetById(long id);
    Task<ResponseFilterDTO<TurnoCajaDTO>> GetAll(FilterDTO? filter);
    Task<List<TurnoCajaDTO>> GetAll();
}
