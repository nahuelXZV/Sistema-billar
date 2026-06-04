using Application.Features.Sales.MetodosPago.Commands;
using Application.Features.Sales.MetodosPago.Queries;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Sales;

public class MetodoPagoController : MainController
{
    private readonly ILogger<MetodoPagoController> _logger;

    public MetodoPagoController(ILogger<MetodoPagoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetMetodosPagoFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetMetodosPagoQuery()));
    }

    [HttpGet("{idMetodoPago}")]
    public async Task<IActionResult> GetById(long idMetodoPago)
    {
        return Ok(await Mediator.Send(new GetMetodoPagoByIdQuery { Id = idMetodoPago }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(MetodoPagoDTO metodoPagoDto)
    {
        return Ok(await Mediator.Send(new CreateMetodoPagoCommand { MetodoPagoDTO = metodoPagoDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(MetodoPagoDTO metodoPagoDto)
    {
        return Ok(await Mediator.Send(new UpdateMetodoPagoCommand { MetodoPagoDTO = metodoPagoDto }));
    }

    [HttpDelete("Delete/{idMetodoPago}")]
    public async Task<IActionResult> Delete(long idMetodoPago)
    {
        return Ok(await Mediator.Send(new DeleteMetodoPagoCommand { MetodoPagoId = idMetodoPago }));
    }
}
