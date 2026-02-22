using Application.Features.Inventory.Lotes.Commands;
using Application.Features.Inventory.Lotes.Queries;
using Domain.DTOs.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class LoteController : MainController
{
    private readonly ILogger<LoteController> _logger;

    public LoteController(ILogger<LoteController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetLotesQuery() { }));
    }

    [HttpGet("{idProd}")]
    public async Task<IActionResult> GetById(long idProd)
    {
        return Ok(await Mediator.Send(new GetLoteByProductoQuery() { IdProducto = idProd }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(LoteDTO loteDto)
    {
        return Ok(await Mediator.Send(new CreateLoteCommand { LoteDTO = loteDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(LoteDTO loteDto)
    {
        return Ok(await Mediator.Send(new UpdateLoteCommand { LoteDTO = loteDto }));
    }

    [HttpDelete("Delete/{idProd}")]
    public async Task<IActionResult> Delete(long idProd)
    {
        return Ok(await Mediator.Send(new DeleteLoteCommand { Id = idProd }));
    }
}
