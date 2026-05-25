using Application.Features.Inventory.ListaPrecio.Commands;
using Application.Features.Inventory.ListaPrecio.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class ListaPreciosController : MainController
{
    private readonly ILogger<ListaPreciosController> _logger;

    public ListaPreciosController(ILogger<ListaPreciosController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetListaPreciosFilterQuery() { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetListaPreciosQuery()));
    }

    [HttpGet("{idAlm}")]
    public async Task<IActionResult> GetById(long idAlm)
    {
        return Ok(await Mediator.Send(new GetListaPreciosByIdQuery() { Id = idAlm }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ListaPrecioDTO listaPrecioDto)
    {
        return Ok(await Mediator.Send(new CreateListaPreciosCommand { ListaPrecioDTO = listaPrecioDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ListaPrecioDTO listaPrecioDto)
    {
        return Ok(await Mediator.Send(new UpdateListaPreciosCommand { ListaPrecioDTO = listaPrecioDto }));
    }

    [HttpDelete("Delete/{idListPrecio}")]
    public async Task<IActionResult> Delete(long idListPrecio)
    {
        return Ok(await Mediator.Send(new DeleteListaPreciosCommand { Id = idListPrecio }));
    }
}
