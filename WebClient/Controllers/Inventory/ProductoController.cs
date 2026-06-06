using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models.Inventory;
using WebClient.Models;
using Domain.Constants;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class ProductoController : MainController
{
    private readonly ILogger<ProductoController> _logger;

    public ProductoController(ViewModelFactory viewModelFactory, ILogger<ProductoController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<ProductoViewModel>();
        //model.IncluirBlazorComponents = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var ListaAlmacenes = await _appServices.ProductoService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });
            return Ok(ListaAlmacenes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<ProductoDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<ProductoViewModel>();
        model.IncluirBlazorComponents = true;

        model.ListadoCategorias = await _appServices.CategoriaService.GetAllSinNivel();
        model.ListadoUnidadesMedidas = await _appServices.UnidadMedidaService.GetAll();
        model.ListaProductos = await _appServices.ProductoService.GetAll();
        model.ListaProductos = model.ListaProductos.Where(p => p.Id != id).ToList();
        model.ListaTipoProducto = new List<SelectOptionDTO<short>>()
        {
            new SelectOptionDTO<short>() { Value = (short)Constantes.TipoProducto.Producto, Label = "Producto" },
            new SelectOptionDTO<short>() { Value = (short)Constantes.TipoProducto.Servicio, Label = "Servicio" },
        };

        if (id != 0)
            model.Producto = await _appServices.ProductoService.GetById(id);
        else
            model.Producto = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.ProductoService.Delete(id);
            this.AddSuccessTempMessage("Producto eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }




}