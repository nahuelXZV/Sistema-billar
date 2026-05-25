using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Controllers.Sales;

public class VendedorController : MainController
{
    private readonly ILogger<VendedorController> _logger;

    public VendedorController(ViewModelFactory viewModelFactory, ILogger<VendedorController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<VendedorViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var listaVendedores = await _appServices.VendedorService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(listaVendedores);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<VendedorDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<VendedorViewModel>();

        model.IncluirBlazorComponents = true;
        model.ListaUsuarios = (await _appServices.UsuarioService.GetAll(new FilterDTO { Limit = 1000 })).Data;
        model.ListaAlmacenes = await _appServices.AlmacenService.GetAll();
        model.ListaPrecios = await _appServices.ListaPreciosService.GetAll();

        if (id != 0)
            model.Vendedor = await _appServices.VendedorService.GetById(id);
        else
            model.Vendedor = new() { Activo = true, ListaAlmacenes = new() };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.VendedorService.Delete(id);
            this.AddSuccessTempMessage("Vendedor eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }
}
