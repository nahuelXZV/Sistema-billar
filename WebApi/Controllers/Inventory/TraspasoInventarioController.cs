using Application.Features.Inventory.TraspasoInventarios.Commands;
using Application.Features.Inventory.TraspasoInventarios.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class TraspasoInventarioController : MainController
{
    private readonly ILogger<TraspasoInventarioController> _logger;

    public TraspasoInventarioController(ILogger<TraspasoInventarioController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetTraspasosInventarioFilterQuery { Filter = filter }));
    }

    [HttpGet("{idTraspaso}")]
    public async Task<IActionResult> GetById(long idTraspaso)
    {
        return Ok(await Mediator.Send(new GetTraspasoInventarioByIdQuery { Id = idTraspaso }));
    }

    [HttpGet("Almacen/{idAlmacen}/Disponibles")]
    public async Task<IActionResult> GetInventariosDisponibles(long idAlmacen)
    {
        return Ok(await Mediator.Send(new GetInventariosDisponiblesByAlmacenQuery { IdAlmacen = idAlmacen }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TraspasoInventarioDTO traspasoInventario)
    {
        traspasoInventario.IdUsuario = IdUsuarioActual;
        return Ok(await Mediator.Send(new CreateTraspasoInventarioCommand
        {
            TraspasoInventarioDTO = traspasoInventario
        }));
    }

    [HttpDelete("Delete/{idTraspaso}")]
    public async Task<IActionResult> Delete(long idTraspaso)
    {
        return Ok(await Mediator.Send(new DeleteTraspasoInventarioCommand
        {
            Id = idTraspaso,
            IdUsuario = IdUsuarioActual
        }));
    }
}
