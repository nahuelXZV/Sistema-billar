using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Contact;
using WebClient.Services;

namespace WebClient.Controllers.Contact;

public class ProveedorController : MainController
{
    public ProveedorController(
        ViewModelFactory viewModelFactory,
        IAppServices services)
        : base(viewModelFactory, services)
    {
    }

    [HttpGet]
    public IActionResult Listado()
    {
        return View(_viewModelFactory.Create<ProveedorViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string search = "",
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0)
    {
        try
        {
            var proveedores = await _appServices.ProveedorService.GetAll(new FilterDTO
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(proveedores);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<ProveedorDTO> { Total = 0, Data = [] });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<ProveedorViewModel>();
        model.IncluirBlazorComponents = true;
        model.ListaProductos = await _appServices.ProductoService.GetAll();

        model.Proveedor = id > 0
            ? await _appServices.ProveedorService.GetById(id)
            : new ProveedorDTO { Activo = true, ListaProductos = [] };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.ProveedorService.Delete(id);
            this.AddSuccessTempMessage("Proveedor eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction(nameof(Listado));
    }
}
