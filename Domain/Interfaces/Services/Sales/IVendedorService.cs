using Domain.DTOs.Sales;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Sales;

public interface IVendedorService
{
    Task<long> Create(VendedorDTO vendedor);
    Task<bool> Update(VendedorDTO vendedor);
    Task<bool> Delete(long id);
    Task<VendedorDTO> GetById(long id);
    Task<VendedorDTO> GetByUsuario(long idUsuario);
    Task<ResponseFilterDTO<VendedorDTO>> GetAll(FilterDTO? filter);
    Task<List<VendedorDTO>> GetAll();
}
