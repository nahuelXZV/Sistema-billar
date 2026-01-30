using Domain.DTOs.Security;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Security;

public interface IUsuarioService
{
    Task<long> Create(UsuarioDTO usuario);
    Task<bool> Update(UsuarioDTO usuario);
    Task<bool> Delete(long id);
    Task<UsuarioDTO> GetById(long id);
    Task<ResponseFilterDTO<UsuarioDTO>> GetAll(FilterDTO? filter);
}
