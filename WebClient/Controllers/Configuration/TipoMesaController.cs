using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Configuration;
using WebClient.Services;

namespace WebClient.Controllers.Configuration;

public class TipoMesaController : MainController
{
    private readonly ILogger<TipoMesaController> _logger;

    public TipoMesaController(ViewModelFactory viewModelFactory, ILogger<TipoMesaController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<TipoMesaViewModel>();
        //model.IncluirBlazorComponents = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ListaAlmacenes = await _appServices.TipoMesaService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(ListaAlmacenes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<TipoMesaDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<TipoMesaViewModel>();

        model.IncluirBlazorComponents = true;
        model.ListaProductos = await _appServices.ProductoService.GetAll();

        if (id != 0)
            model.TipoMesa = await _appServices.TipoMesaService.GetById(id);
        else
            model.TipoMesa = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.TipoMesaService.Delete(id);
            this.AddSuccessTempMessage("Tipo de mesa eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }
}
