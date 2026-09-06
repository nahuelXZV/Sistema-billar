using System.Security.AccessControl;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebClient.Common.Utils;
using WebClient.Configs;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Controllers.Sales;

public class VentaController : MainController
{
    private readonly ILogger<VentaController> _logger;

    public VentaController(ViewModelFactory viewModelFactory, ILogger<VentaController> logger, IAppServices services, IOptions<AdminConfig> adminConfig)
        : base(viewModelFactory, services)
    {
        _logger = logger;
        _adminConfig = adminConfig.Value;
    }

    [HttpGet]
    public async Task<IActionResult> VentaDirecta()
    {
        var (model, error) = await CrearModeloConTurnoActivo();
        if (error is not null) return error;

        var categoriasBase = await _appServices.CategoriaService.GetCategoriasBase();
        model.PuntoVenta = PuntoVentaUtils.Create(categoriasBase, model.Vendedor);
        await ConfigurarClientesAsync(model.PuntoVenta);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VentaMesas()
    {
        var (model, error) = await CrearModeloConTurnoActivo();
        if (error is not null) return error;

        model.Mesas = await _appServices.MesasService.GetAll();

        var categoriasBase = await _appServices.CategoriaService.GetCategoriasBase();
        model.PuntoVenta = PuntoVentaUtils.Create(categoriasBase, model.Vendedor);
        await ConfigurarClientesAsync(model.PuntoVenta);
        return View(model);
    }

    private async Task ConfigurarClientesAsync(PuntoVentaViewModel puntoVenta)
    {
        var clientes = await _appServices.ClienteService.GetAll();
        var clientePredeterminado = clientes.FirstOrDefault(cliente => cliente.Id == _adminConfig.Personalizaciones.IdClienteDefault)
            ?? throw new InvalidOperationException("El cliente predeterminado no está disponible.");

        puntoVenta.Clientes = clientes;
        puntoVenta.IdClienteDefault = clientePredeterminado.Id;
        puntoVenta.ClienteSeleccionado = clientePredeterminado;
    }

    private async Task<(VentaViewModel Model, IActionResult? Error)> CrearModeloConTurnoActivo()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        model.IncluirBlazorComponents = true;
        model.Vendedor = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);

        if (model.Vendedor.Id <= 0)
        {
            this.AddErrorTempMessage("El usuario no tiene un vendedor activo asignado.");
            return (model, RedirectToAction("Listado", "TurnoCaja"));
        }

        if (!await _appServices.TurnoCajaService.TieneActivo(model.Vendedor.Id))
        {
            this.AddErrorTempMessage("Debe abrir un turno de caja antes de ingresar a ventas.");
            return (model, RedirectToAction("Listado", "TurnoCaja"));
        }

        return (model, null);
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ventas = await _appServices.VentaService.GetAll(new FilterDTO
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(ventas);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<VentaDTO> { Total = 0, Data = new() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Ver([FromQuery] long id)
    {
        try
        {
            var model = _viewModelFactory.Create<VentaViewModel>();
            model.IncluirBlazorComponents = true;
            model.Venta = await _appServices.VentaService.GetById(id);
            return View(model);
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
            return RedirectToAction(nameof(Listado));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.VentaService.Delete(id);
            this.AddSuccessTempMessage("Venta eliminada correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction(nameof(Listado));
    }


}
