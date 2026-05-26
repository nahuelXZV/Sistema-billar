using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface IVentaService
{
    Task<long> Create(VentaDTO venta);
    Task<bool> Update(VentaDTO venta);
    Task<bool> Delete(long id);
    Task<VentaDTO> GetById(long id);
    Task<ResponseFilterDTO<VentaDTO>> GetAll(FilterDTO? filter);
    Task<List<VentaDTO>> GetAll();
}
