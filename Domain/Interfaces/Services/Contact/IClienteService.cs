using Domain.DTOs.Contact;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Contact;

public interface IClienteService
{
    Task<long> Create(ClienteDTO cliente);
    Task<bool> Update(ClienteDTO cliente);
    Task<bool> Delete(long id);
    Task<ClienteDTO> GetById(long id);
    Task<ResponseFilterDTO<ClienteDTO>> GetAll(FilterDTO? filter);
    Task<List<ClienteDTO>> GetAll();
}
