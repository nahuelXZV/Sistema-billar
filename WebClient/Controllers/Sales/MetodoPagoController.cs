using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Controllers.Sales;

public class MetodoPagoController : MainController
{
    private readonly ILogger<MetodoPagoController> _logger;

    public MetodoPagoController(ViewModelFactory viewModelFactory, ILogger<MetodoPagoController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<MetodoPagoViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var metodosPago = await _appServices.MetodoPagoService.GetAll(new FilterDTO
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(metodosPago);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<MetodoPagoDTO> { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<MetodoPagoViewModel>();

        model.IncluirBlazorComponents = true;

        if (id != 0)
            model.MetodoPago = await _appServices.MetodoPagoService.GetById(id);
        else
            model.MetodoPago = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.MetodoPagoService.Delete(id);
            this.AddSuccessTempMessage("Metodo de pago eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction("Listado");
    }
}
