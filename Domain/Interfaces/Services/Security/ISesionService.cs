using Domain.DTOs.Security;
using Domain.DTOs.Security.Request;

namespace Domain.Interfaces.Services.Security;

public interface ISesionService
{
    Task<UsuarioDTO> Login(RequestLoginDTO model);
}
