using Domain.Constants;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Models;
using WebClient.Models.Inventory;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class TransaccionInventarioController : MainController
{
    private readonly ILogger<TransaccionInventarioController> _logger;

    public TransaccionInventarioController(ViewModelFactory viewModelFactory, ILogger<TransaccionInventarioController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<TransaccionInventarioViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var listaStock = await _appServices.TransaccionInventarioService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(listaStock);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<TransaccionInventarioDetalleDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<TransaccionInventarioViewModel>();

        model.IncluirBlazorComponents = true;
        model.ListadoAlmacen = await _appServices.AlmacenService.GetAll();
        model.ListadoProductos = await _appServices.ProductoService.GetAll();
        model.ListadoProductos = model.ListadoProductos.Where(p => p.Tipo == (short)Constantes.TipoProducto.Producto).ToList();
        model.ListadoTipos = new List<SelectOptionDTO<short>>()
        {
            new SelectOptionDTO<short>() { Value = (short)Constantes.TipoTransaccionInventario.Salida, Label = "Salida"},
            new SelectOptionDTO<short>() { Value = (short)Constantes.TipoTransaccionInventario.Ingreso, Label = "Ingreso"},
            new SelectOptionDTO<short>() { Value = (short)Constantes.TipoTransaccionInventario.Merma, Label = "Merma"},
        };
        model.Transaccion = new TransaccionInventarioDTO() { };
        return View(model);
    }
}