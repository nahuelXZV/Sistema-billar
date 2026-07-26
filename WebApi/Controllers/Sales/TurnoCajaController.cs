using Application.Features.Sales.TurnosCaja.Commands;
using Application.Features.Sales.TurnosCaja.Queries;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Sales;

public class TurnoCajaController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetTurnosCajaFilterQuery
        {
            Filter = filter,
            IdUsuario = IdUsuarioActual
        }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetTurnosCajaQuery()));
    }

    [HttpGet("TieneActivo/{idVendedor}")]
    public async Task<IActionResult> TieneActivo(long idVendedor)
    {
        return Ok(await Mediator.Send(new TieneTurnoCajaActivoQuery
        {
            IdVendedor = idVendedor
        }));
    }

    [HttpGet("{idTurnoCaja}")]
    public async Task<IActionResult> GetById(long idTurnoCaja)
    {
        return Ok(await Mediator.Send(new GetTurnoCajaByIdQuery { Id = idTurnoCaja }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TurnoCajaDTO turnoCajaDTO)
    {
        return Ok(await Mediator.Send(new CreateTurnoCajaCommand
        {
            TurnoCajaDTO = turnoCajaDTO,
            IdUsuario = IdUsuarioActual
        }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(TurnoCajaDTO turnoCajaDTO)
    {
        return Ok(await Mediator.Send(new UpdateTurnoCajaCommand { TurnoCajaDTO = turnoCajaDTO }));
    }

    [HttpDelete("Delete/{idTurnoCaja}")]
    public async Task<IActionResult> Delete(long idTurnoCaja)
    {
        return Ok(await Mediator.Send(new DeleteTurnoCajaCommand
        {
            Id = idTurnoCaja,
            IdUsuario = IdUsuarioActual
        }));
    }
}
