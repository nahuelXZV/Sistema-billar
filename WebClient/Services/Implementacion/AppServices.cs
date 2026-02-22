
using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Security;

namespace WebClient.Services.Implementacion;

public class AppServices : IAppServices
{
    private readonly ILogger<AppServices> _logger;

    private readonly IServiceProvider _serviceProvider;

    private ISesionService _sesionService;
    private IPerfilService _perfilService;
    private IModuloService _moduloService;
    private IUsuarioService _usuarioService;

    private IAlmacenService _almacenService;
    private ICategoriaService _categoriaService;
    private IUnidadMedidaService _unidadMedidaService;
    private IProductoService _productoService;
    private IInventarioService _inventarioService;
    private ITransaccionInventarioService _transaccionInventarioService;
    private IListaPreciosService _listaPrecioService;
    private ILoteService _loteService;

    public AppServices(IServiceProvider serviceProvider, ILogger<AppServices> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    #region SEGURITY
    public ISesionService SesionService => _sesionService ??= _serviceProvider.GetService<ISesionService>();
    public IPerfilService PerfilService => _perfilService ??= _serviceProvider.GetService<IPerfilService>();
    public IModuloService ModuloService => _moduloService ??= _serviceProvider.GetService<IModuloService>();
    public IUsuarioService UsuarioService => _usuarioService ??= _serviceProvider.GetService<IUsuarioService>();
    #endregion


    #region INVENTORY
    public IAlmacenService AlmacenService => _almacenService ??= _serviceProvider.GetService<IAlmacenService>();
    public ICategoriaService CategoriaService => _categoriaService ??= _serviceProvider.GetService<ICategoriaService>();
    public IUnidadMedidaService UnidadMedidaService => _unidadMedidaService ??= _serviceProvider.GetService<IUnidadMedidaService>();
    public IProductoService ProductoService => _productoService ??= _serviceProvider.GetService<IProductoService>();
    public IInventarioService InventarioService => _inventarioService ??= _serviceProvider.GetService<IInventarioService>();
    public ITransaccionInventarioService TransaccionInventarioService => _transaccionInventarioService ??= _serviceProvider.GetService<ITransaccionInventarioService>();
    public IListaPreciosService ListaPreciosService => _listaPrecioService ??= _serviceProvider.GetService<IListaPreciosService>();
    public ILoteService LoteService => _loteService ??= _serviceProvider.GetService<ILoteService>();
    #endregion

    #region CONFIGURACION

    #endregion
}
