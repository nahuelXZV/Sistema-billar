using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Configuration;

public interface ITipoMesaService
{
    Task<long> Create(TipoMesaDTO tipo);
    Task<bool> Update(TipoMesaDTO tipo);
    Task<bool> Delete(long id);
    Task<TipoMesaDTO> GetById(long id);
    Task<ResponseFilterDTO<TipoMesaDTO>> GetAll(FilterDTO? filter);
    Task<List<TipoMesaDTO>> GetAll();
}
