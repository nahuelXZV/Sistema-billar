using Domain.DTOs.Contact;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Contact;

public interface IProveedorService
{
    Task<long> Create(ProveedorDTO proveedor);
    Task<bool> Update(ProveedorDTO proveedor);
    Task<bool> Delete(long id);
    Task<ProveedorDTO> GetById(long id);
    Task<ResponseFilterDTO<ProveedorDTO>> GetAll(FilterDTO? filter);
    Task<List<ProveedorDTO>> GetAll();
}
