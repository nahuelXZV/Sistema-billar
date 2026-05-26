using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface IUsoMesaService
{
    Task<long> Create(UsoMesaDTO usoMesa);
    Task<bool> Update(UsoMesaDTO usoMesa);
    Task<bool> Delete(long id);
    Task<UsoMesaDTO> GetById(long id);
    Task<ResponseFilterDTO<UsoMesaDTO>> GetAll(FilterDTO? filter);
    Task<List<UsoMesaDTO>> GetAll();
}
