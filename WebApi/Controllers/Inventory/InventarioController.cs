using Application.Features.Inventory.Inventarios.Queries;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class InventarioController : MainController
{
    private readonly ILogger<InventarioController> _logger;

    public InventarioController(ILogger<InventarioController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFilter([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetInventariosFilterQuery() { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetInventariosQuery() { }));
    }

    [HttpGet("{idProd}")]
    public async Task<IActionResult> GetById(long idProd)
    {
        return Ok(await Mediator.Send(new GetInventarioByProductoQuery() { IdProducto = idProd }));
    }
}