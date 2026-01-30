using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Security;

namespace WebClient.Services;

public interface IAppServices
{
    public ISesionService SesionService { get; }
    public IPerfilService PerfilService { get; }
    public IModuloService ModuloService { get; }
    public IUsuarioService UsuarioService { get; }
    public IAlmacenService AlmacenService { get; }

}
