using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface IMetodoPagoService
{
    Task<long> Create(MetodoPagoDTO metodoPago);
    Task<bool> Update(MetodoPagoDTO metodoPago);
    Task<bool> Delete(long id);
    Task<MetodoPagoDTO> GetById(long id);
    Task<ResponseFilterDTO<MetodoPagoDTO>> GetAll(FilterDTO? filter);
    Task<List<MetodoPagoDTO>> GetAll();
}
