using Application.Features.Configuration.Mesas.Commands;
using Application.Features.Configuration.Mesas.Queries;
using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration;

public class MesasController : MainController
{
    private readonly ILogger<MesasController> _logger;

    public MesasController(ILogger<MesasController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetMesasFilterQuery() { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetMesasQuery() { }));
    }

    [HttpGet("{idMesa}")]
    public async Task<IActionResult> GetById(long idMesa)
    {
        return Ok(await Mediator.Send(new GetMesaByIdQuery() { Id = idMesa }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(MesaDTO mesaDto)
    {
        return Ok(await Mediator.Send(new CreateMesaCommand { MesaDto = mesaDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(MesaDTO mesaDto)
    {
        return Ok(await Mediator.Send(new UpdateMesaCommand { MesaDto = mesaDto }));
    }

    [HttpDelete("Delete/{idMesa}")]
    public async Task<IActionResult> Delete(long idMesa)
    {
        return Ok(await Mediator.Send(new DeleteMesaCommand { Id = idMesa }));
    }
}
