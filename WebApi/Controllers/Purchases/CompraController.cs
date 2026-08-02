using Application.Features.Purchases.Compras.Commands;
using Application.Features.Purchases.Compras.Queries;
using Domain.DTOs.Purchases;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Purchases;

public class CompraController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetComprasFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetComprasQuery()));
    }

    [HttpGet("{idCompra}")]
    public async Task<IActionResult> GetById(long idCompra)
    {
        return Ok(await Mediator.Send(new GetCompraByIdQuery { Id = idCompra }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompraDTO compraDTO)
    {
        compraDTO.IdUsuario = IdUsuarioActual;
        return Ok(await Mediator.Send(new CreateCompraCommand { CompraDTO = compraDTO }));
    }

    [HttpPost("{idCompra}/Anular")]
    public async Task<IActionResult> Anular(long idCompra, AnularCompraDTO solicitud)
    {
        return Ok(await Mediator.Send(new AnularCompraCommand
        {
            IdCompra = idCompra,
            IdUsuarioAnulacion = IdUsuarioActual,
            Motivo = solicitud.Motivo
        }));
    }
}
