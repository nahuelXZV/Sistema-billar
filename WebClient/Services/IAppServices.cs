using Domain.Interfaces.Services.Configuration;
using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Sales;
using Domain.Interfaces.Services.Security;

namespace WebClient.Services;

public interface IAppServices
{
    public ISesionService SesionService { get; }
    public IPerfilService PerfilService { get; }
    public IModuloService ModuloService { get; }
    public IUsuarioService UsuarioService { get; }

    public IAlmacenService AlmacenService { get; }
    public ICategoriaService CategoriaService { get; }
    public IUnidadMedidaService UnidadMedidaService { get; }
    public IProductoService ProductoService { get; }
    public IInventarioService InventarioService { get; }
    public ITransaccionInventarioService TransaccionInventarioService { get; }
    public IListaPreciosService ListaPreciosService { get; }
    public ILoteService LoteService { get; }

    public ITipoMesaService TipoMesaService { get; }
    public IMesasService MesasService { get; }
    public IVendedorService VendedorService { get; }
}
