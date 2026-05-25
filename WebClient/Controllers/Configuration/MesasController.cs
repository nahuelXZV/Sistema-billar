using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Configuration;
using WebClient.Services;

namespace WebClient.Controllers.Configuration;

public class MesasController : MainController
{
    private readonly ILogger<MesasController> _logger;

    public MesasController(ViewModelFactory viewModelFactory, ILogger<MesasController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<MesasViewModel>();
        //model.IncluirBlazorComponents = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ListaAlmacenes = await _appServices.MesasService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(ListaAlmacenes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<MesaDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<MesasViewModel>();

        model.IncluirBlazorComponents = true;
        model.ListaTiposMesa = await _appServices.TipoMesaService.GetAll();

        if (id != 0)
            model.Mesa = await _appServices.MesasService.GetById(id);
        else
            model.Mesa = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.MesasService.Delete(id);
            this.AddSuccessTempMessage("Mesa eliminada correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }
}
