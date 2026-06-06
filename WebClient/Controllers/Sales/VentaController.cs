using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Controllers.Sales;

public class VentaController : MainController
{
    private readonly ILogger<VentaController> _logger;

    public VentaController(ViewModelFactory viewModelFactory, ILogger<VentaController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string search = "",
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0)
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

    [HttpGet]
    public async Task<IActionResult> VentaDirecta()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        model.IncluirBlazorComponents = true;
        model.Vendedor = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);
        var categoriasBase = await _appServices.CategoriaService.GetCategoriasBase();
        model.PuntoVenta = PuntoVentaUtils.Create(categoriasBase, model.Vendedor);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VentaMesas()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        model.IncluirBlazorComponents = true;
        model.Vendedor = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);
        model.Mesas = await _appServices.MesasService.GetAll();
        return View(model);
    }
}
