using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Models;
using WebClient.Models.Inventory;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class InventarioController : MainController
{
    private readonly ILogger<InventarioController> _logger;

    public InventarioController(ViewModelFactory viewModelFactory, ILogger<InventarioController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<InventarioViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var listaStock = await _appServices.InventarioService.GetAll(new FilterDTO()
            {
                Limit = limit,
                Offset = offset,
                Search = search
            });

            return Ok(listaStock);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<InventarioDTO>() { Total = 0, Data = new() });
        }
    }
}