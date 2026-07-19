using Application.Features.Sales.Ventas.Commands;
using Application.Features.Sales.Ventas.Queries;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Sales;

public class VentaController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetVentasFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetVentasQuery()));
    }

    [HttpGet("{idVenta}")]
    public async Task<IActionResult> GetById(long idVenta)
    {
        return Ok(await Mediator.Send(new GetVentaByIdQuery { Id = idVenta }));
    }

    [HttpGet("GetMontoVendidoVendedor/{idVendedor}/{idTurnoCaja}")]
    public async Task<IActionResult> GetMontoVendidoVendedor(long idVendedor, long idTurnoCaja)
    {
        return Ok(await Mediator.Send(new GetMontoVendidoVendedorQuery
        {
            IdVendedor = idVendedor,
            IdTurnoCaja = idTurnoCaja
        }));
    }

    [HttpGet("GetMontosVendidosPorMetodoPago/{idVendedor}/{idTurnoCaja}")]
    public async Task<IActionResult> GetMontosVendidosPorMetodoPago(long idVendedor, long idTurnoCaja)
    {
        return Ok(await Mediator.Send(new GetMontosVendidosPorMetodoPagoQuery
        {
            IdVendedor = idVendedor,
            IdTurnoCaja = idTurnoCaja
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(VentaDTO ventaDTO)
    {
        return Ok(await Mediator.Send(new CreateVentaCommand { VentaDTO = ventaDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(VentaDTO ventaDTO)
    {
        return Ok(await Mediator.Send(new UpdateVentaCommand { VentaDTO = ventaDTO }));
    }

    [HttpDelete("Delete/{idVenta}")]
    public async Task<IActionResult> Delete(long idVenta)
    {
        return Ok(await Mediator.Send(new DeleteVentaCommand
        {
            IdVenta = idVenta,
            IdUsuario = IdUsuarioActual
        }));
    }
}
