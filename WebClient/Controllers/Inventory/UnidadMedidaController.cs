using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models.Inventory;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class UnidadMedidaController : MainController
{
    private readonly ILogger<UnidadMedidaController> _logger;

    public UnidadMedidaController(ViewModelFactory viewModelFactory, ILogger<UnidadMedidaController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<UnidadMedidaViewModel>();
        //model.IncluirBlazorComponents = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ListaAlmacenes = await _appServices.UnidadMedidaService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(ListaAlmacenes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<UnidadMedidaDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<UnidadMedidaViewModel>();

        model.IncluirBlazorComponents = true;

        if (id != 0)
            model.Unidad = await _appServices.UnidadMedidaService.GetById(id);
        else
            model.Unidad = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.UnidadMedidaService.Delete(id);
            this.AddSuccessTempMessage("Unidad Medida eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }




}
