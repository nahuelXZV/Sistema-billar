using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Inventory;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class ListaPreciosController : MainController
{
    private readonly ILogger<ListaPreciosController> _logger;

    public ListaPreciosController(ViewModelFactory viewModelFactory, ILogger<ListaPreciosController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<ListaPreciosViewModel>();
        //model.IncluirBlazorComponents = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ListaAlmacenes = await _appServices.ListaPreciosService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });
            return Ok(ListaAlmacenes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<ListaPrecioDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<ListaPreciosViewModel>();
        model.IncluirBlazorComponents = true;
        model.ListaProductos = await _appServices.ProductoService.GetAll();

        if (id != 0)
            model.ListaPrecio = await _appServices.ListaPreciosService.GetById(id);
        else
            model.ListaPrecio = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.ListaPreciosService.Delete(id);
            this.AddSuccessTempMessage("Lista de precios eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }




}