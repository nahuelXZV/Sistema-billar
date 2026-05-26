using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface IOrdenVentaService
{
    Task<long> Create(OrdenVentaDTO ordenVenta);
    Task<bool> Update(OrdenVentaDTO ordenVenta);
    Task<bool> Delete(long id);
    Task<OrdenVentaDTO> GetById(long id);
    Task<ResponseFilterDTO<OrdenVentaDTO>> GetAll(FilterDTO? filter);
    Task<List<OrdenVentaDTO>> GetAll();
}
