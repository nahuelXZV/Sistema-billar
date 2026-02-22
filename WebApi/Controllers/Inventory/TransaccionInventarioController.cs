using Application.Features.Inventory.Inventarios.Queries;
using Application.Features.Inventory.Productos.Commands;
using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Features.Inventory.TransaccionInventarios.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class TransaccionInventarioController : MainController
{
    private readonly ILogger<TransaccionInventarioController> _logger;

    public TransaccionInventarioController(ILogger<TransaccionInventarioController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFilter([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetTransaccionInventarioFilterQuery() { Filter = filter }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransaccionInventarioDTO transaccionInventario)
    {
        return Ok(await Mediator.Send(new CreateTransaccionInventarioCommand { TransaccionInventarioDTO = transaccionInventario }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(TransaccionInventarioDTO transaccionInventario)
    {
        return Ok(await Mediator.Send(new UpdateTransaccionInventarioCommand { TransaccionInventarioDTO = transaccionInventario }));
    }

}