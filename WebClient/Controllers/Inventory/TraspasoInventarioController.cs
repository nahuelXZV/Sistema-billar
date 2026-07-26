using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Inventory;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class TraspasoInventarioController : MainController
{
    public TraspasoInventarioController(ViewModelFactory viewModelFactory, IAppServices services)
        : base(viewModelFactory, services)
    {
    }

    [HttpGet]
    public IActionResult Listado()
    {
        return View(_viewModelFactory.Create<TraspasoInventarioViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            return Ok(await _appServices.TraspasoInventarioService.GetAll(new FilterDTO
            {
                Limit = limit,
                Offset = offset,
                Search = search
            }));
        }
        catch
        {
            return Ok(new ResponseFilterDTO<TraspasoInventarioDTO> { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear()
    {
        var model = _viewModelFactory.Create<TraspasoInventarioViewModel>();
        model.IncluirBlazorComponents = true;
        model.ListadoAlmacenes = await _appServices.AlmacenService.GetAll();
        model.Traspaso = new TraspasoInventarioDTO { Fecha = DateTime.Now };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.TraspasoInventarioService.Delete(id);
            this.AddSuccessTempMessage("Traspaso revertido correctamente.");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction(nameof(Listado));
    }
}
