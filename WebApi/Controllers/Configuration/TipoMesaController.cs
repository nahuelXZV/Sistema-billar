using Application.Features.Configuration.TipoMesas.Commands;
using Application.Features.Configuration.TipoMesas.Queries;
using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration;

public class TipoMesaController : MainController
{
    private readonly ILogger<TipoMesaController> _logger;

    public TipoMesaController(ILogger<TipoMesaController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetTipoMesaFilterQuery() { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetTipoMesasQuery() { }));
    }

    [HttpGet("{idTipo}")]
    public async Task<IActionResult> GetById(long idTipo)
    {
        return Ok(await Mediator.Send(new GetTipoMesaByIdQuery() { Id = idTipo }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TipoMesaDTO tipoMesaDto)
    {
        return Ok(await Mediator.Send(new CreateTipoMesaCommand { TipoMesaDto = tipoMesaDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(TipoMesaDTO almacenDTO)
    {
        return Ok(await Mediator.Send(new UpdateTipoMesaCommand { TipoMesaDto = almacenDTO }));
    }

    [HttpDelete("Delete/{idTipo}")]
    public async Task<IActionResult> Delete(long idTipo)
    {
        return Ok(await Mediator.Send(new DeleteTipoMesaCommand { Id = idTipo }));
    }
}
