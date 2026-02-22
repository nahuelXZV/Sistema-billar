using Application.Features.Inventory.Almacenes.Commands;
using Application.Features.Inventory.Almacenes.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class AlmacenController : MainController
{
    private readonly ILogger<AlmacenController> _logger;

    public AlmacenController(ILogger<AlmacenController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetAlmacenesFilterQuery() { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetAlmacenesQuery() { }));
    }

    [HttpGet("{idAlm}")]
    public async Task<IActionResult> GetById(long idAlm)
    {
        return Ok(await Mediator.Send(new GetAlmacenByIdQuery() { Id = idAlm }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(AlmacenDTO almacenDTO)
    {
        return Ok(await Mediator.Send(new CreateAlmacenCommand { AlmacenDTO = almacenDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(AlmacenDTO almacenDTO)
    {
        return Ok(await Mediator.Send(new UpdateAlmacenCommand { AlmacenDTO = almacenDTO }));
    }

    [HttpDelete("Delete/{idAlm}")]
    public async Task<IActionResult> Delete(long idAlm)
    {
        return Ok(await Mediator.Send(new DeleteAlmacenCommand { AlmacenId = idAlm }));
    }
}
