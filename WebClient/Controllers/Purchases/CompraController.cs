using Domain.DTOs.Purchases;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Purchases;
using WebClient.Services;

namespace WebClient.Controllers.Purchases;

public class CompraController : MainController
{
    public CompraController(ViewModelFactory viewModelFactory, IAppServices services)
        : base(viewModelFactory, services)
    {
    }

    [HttpGet]
    public IActionResult Listado()
    {
        return View(_viewModelFactory.Create<CompraViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            return Ok(await _appServices.CompraService.GetAll(new FilterDTO
            {
                Limit = limit,
                Offset = offset,
                Search = search
            }));
        }
        catch
        {
            return Ok(new ResponseFilterDTO<CompraDTO> { Total = 0, Data = [] });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear()
    {
        var model = _viewModelFactory.Create<CompraViewModel>();
        model.IncluirBlazorComponents = true;
        model.ListaProveedores = (await _appServices.ProveedorService.GetAll())
            .Where(proveedor => proveedor.Activo)
            .OrderBy(proveedor => proveedor.NombreComercial)
            .ToList();
        model.ListaAlmacenes = await _appServices.AlmacenService.GetAll();
        model.ListaProductos = (await _appServices.ProductoService.GetAll())
            .Where(producto => producto.Activo && producto.Tipo == 1)
            .OrderBy(producto => producto.Nombre)
            .ToList();
        model.Compra = new CompraDTO
        {
            Fecha = DateTime.Now,
            IdUsuario = model.IdUsuarioLoggedIn,
            IdempotencyKey = Guid.NewGuid(),
            ListaDetalles = []
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Ver(long id)
    {
        var model = _viewModelFactory.Create<CompraViewModel>();
        model.Compra = await _appServices.CompraService.GetById(id);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Anular([FromForm] long id, [FromForm] string motivo)
    {
        try
        {
            await _appServices.CompraService.Anular(id, motivo);
            this.AddSuccessTempMessage("Compra anulada correctamente.");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction(nameof(Ver), new { id });
    }
}
