
using Domain.Interfaces.Services.Contact;
using Domain.Interfaces.Services.Configuration;
using Domain.Interfaces.Services.General;
using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Purchases;
using Domain.Interfaces.Services.Sales;
using Domain.Interfaces.Services.Security;
using Domain.Interfaces.Services.Shared;

namespace WebClient.Services.Implementacion;

public class AppServices : IAppServices
{
    private readonly ILogger<AppServices> _logger;

    private readonly IServiceProvider _serviceProvider;

    private ISesionService _sesionService;
    private IPerfilService _perfilService;
    private IModuloService _moduloService;
    private IUsuarioService _usuarioService;
    private IClienteService _clienteService;
    private IProveedorService _proveedorService;
    private IDashboardService _dashboardService;

    private IAlmacenService _almacenService;
    private ICategoriaService _categoriaService;
    private IUnidadMedidaService _unidadMedidaService;
    private IProductoService _productoService;
    private IInventarioService _inventarioService;
    private ITransaccionInventarioService _transaccionInventarioService;
    private ITraspasoInventarioService _traspasoInventarioService;
    private IListaPreciosService _listaPrecioService;
    private ILoteService _loteService;
    private ICompraService _compraService;
    private ITipoMesaService _tipoMesaService;
    private IMesasService _mesasService;
    private IVendedorService _vendedorService;
    private IMetodoPagoService _metodoPagoService;
    private ITurnoCajaService _turnoCajaService;
    private IUsoMesaService _usoMesaService;
    private IOrdenVentaService _ordenVentaService;
    private IOrdenMesaService _ordenMesaService;
    private IVentaService _ventaService;
    private IArchivoService _archivoService;

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
    public IClienteService ClienteService => _clienteService ??= _serviceProvider.GetService<IClienteService>();
    public IProveedorService ProveedorService => _proveedorService ??= _serviceProvider.GetService<IProveedorService>();
    public IDashboardService DashboardService => _dashboardService ??= _serviceProvider.GetService<IDashboardService>();
    #endregion


    #region INVENTORY
    public IAlmacenService AlmacenService => _almacenService ??= _serviceProvider.GetService<IAlmacenService>();
    public ICategoriaService CategoriaService => _categoriaService ??= _serviceProvider.GetService<ICategoriaService>();
    public IUnidadMedidaService UnidadMedidaService => _unidadMedidaService ??= _serviceProvider.GetService<IUnidadMedidaService>();
    public IProductoService ProductoService => _productoService ??= _serviceProvider.GetService<IProductoService>();
    public IInventarioService InventarioService => _inventarioService ??= _serviceProvider.GetService<IInventarioService>();
    public ITransaccionInventarioService TransaccionInventarioService => _transaccionInventarioService ??= _serviceProvider.GetService<ITransaccionInventarioService>();
    public ITraspasoInventarioService TraspasoInventarioService => _traspasoInventarioService ??= _serviceProvider.GetService<ITraspasoInventarioService>();
    public IListaPreciosService ListaPreciosService => _listaPrecioService ??= _serviceProvider.GetService<IListaPreciosService>();
    public ILoteService LoteService => _loteService ??= _serviceProvider.GetService<ILoteService>();
    public ICompraService CompraService => _compraService ??= _serviceProvider.GetService<ICompraService>();
    #endregion

    #region CONFIGURACION
    public ITipoMesaService TipoMesaService => _tipoMesaService ??= _serviceProvider.GetService<ITipoMesaService>();
    public IMesasService MesasService => _mesasService ??= _serviceProvider.GetService<IMesasService>();
    #endregion

    #region SALES
    public IVendedorService VendedorService => _vendedorService ??= _serviceProvider.GetService<IVendedorService>();
    public IMetodoPagoService MetodoPagoService => _metodoPagoService ??= _serviceProvider.GetService<IMetodoPagoService>();
    public ITurnoCajaService TurnoCajaService => _turnoCajaService ??= _serviceProvider.GetService<ITurnoCajaService>();
    public IUsoMesaService UsoMesaService => _usoMesaService ??= _serviceProvider.GetService<IUsoMesaService>();
    public IOrdenVentaService OrdenVentaService => _ordenVentaService ??= _serviceProvider.GetService<IOrdenVentaService>();
    public IOrdenMesaService OrdenMesaService => _ordenMesaService ??= _serviceProvider.GetService<IOrdenMesaService>();
    public IVentaService VentaService => _ventaService ??= _serviceProvider.GetService<IVentaService>();
    #endregion

    #region SHARED
    public IArchivoService ArchivoService => _archivoService ??= _serviceProvider.GetService<IArchivoService>();
    #endregion
}
