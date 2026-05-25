using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Contact;
using WebClient.Services;

namespace WebClient.Controllers.Contact;

public class ClienteController : MainController
{
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(ViewModelFactory viewModelFactory, ILogger<ClienteController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<ClienteViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var listaClientes = await _appServices.ClienteService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(listaClientes);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<ClienteDTO>() { Total = 0, Data = new() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        var model = _viewModelFactory.Create<ClienteViewModel>();

        model.IncluirBlazorComponents = true;

        if (id != 0)
            model.Cliente = await _appServices.ClienteService.GetById(id);
        else
            model.Cliente = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.ClienteService.Delete(id);
            this.AddSuccessTempMessage("Cliente eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }
}
