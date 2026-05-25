using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Configuration;

public interface IMesasService
{
    Task<long> Create(MesaDTO mesa);
    Task<bool> Update(MesaDTO mesa);
    Task<bool> Delete(long id);
    Task<MesaDTO> GetById(long id);
    Task<ResponseFilterDTO<MesaDTO>> GetAll(FilterDTO? filter);
    Task<List<MesaDTO>> GetAll();
}
