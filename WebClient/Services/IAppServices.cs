using Domain.Interfaces.Services.Contact;
using Domain.Interfaces.Services.Configuration;
using Domain.Interfaces.Services.General;
using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Sales;
using Domain.Interfaces.Services.Security;
using Domain.Interfaces.Services.Shared;

namespace WebClient.Services;

public interface IAppServices
{
    public ISesionService SesionService { get; }
    public IPerfilService PerfilService { get; }
    public IModuloService ModuloService { get; }
    public IUsuarioService UsuarioService { get; }
    public IClienteService ClienteService { get; }
    public IDashboardService DashboardService { get; }

    public IAlmacenService AlmacenService { get; }
    public ICategoriaService CategoriaService { get; }
    public IUnidadMedidaService UnidadMedidaService { get; }
    public IProductoService ProductoService { get; }
    public IInventarioService InventarioService { get; }
    public ITransaccionInventarioService TransaccionInventarioService { get; }
    public ITraspasoInventarioService TraspasoInventarioService { get; }
    public IListaPreciosService ListaPreciosService { get; }
    public ILoteService LoteService { get; }

    public ITipoMesaService TipoMesaService { get; }
    public IMesasService MesasService { get; }
    public IVendedorService VendedorService { get; }
    public IMetodoPagoService MetodoPagoService { get; }
    public ITurnoCajaService TurnoCajaService { get; }
    public IUsoMesaService UsoMesaService { get; }
    public IOrdenVentaService OrdenVentaService { get; }
    public IOrdenMesaService OrdenMesaService { get; }
    public IVentaService VentaService { get; }
    public IArchivoService ArchivoService { get; }
}
