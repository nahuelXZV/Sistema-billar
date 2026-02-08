using Application.Features.Inventory.UnidadesMedidas.Commands;
using Application.Features.Inventory.UnidadesMedidas.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class UnidadMedidaController : MainController
{
    private readonly ILogger<UnidadMedidaController> _logger;

    public UnidadMedidaController(ILogger<UnidadMedidaController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetUnidadesMedidasFilterQuery() { Filter = filter }));
    }
    
    [HttpGet("GetAll")]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetUnidadesMedidasQuery() { }));
    }

    [HttpGet("{idUnd}")]
    public async Task<IActionResult> GetById(long idUnd)
    {
        return Ok(await Mediator.Send(new GetUnidadMedidaByIdQuery() { Id = idUnd }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UnidadMedidaDTO unidadDto)
    {
        return Ok(await Mediator.Send(new CreateUnidadMedidaCommand { UnidadMedidaDTO = unidadDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UnidadMedidaDTO unidadDto)
    {
        return Ok(await Mediator.Send(new UpdateUnidadMedidaCommand { UnidadMedidaDTO = unidadDto }));
    }

    [HttpDelete("Delete/{idUnd}")]
    public async Task<IActionResult> Delete(long idUnd)
    {
        return Ok(await Mediator.Send(new DeleteUnidadMedidaCommand { UnidadId = idUnd }));
    }
}
