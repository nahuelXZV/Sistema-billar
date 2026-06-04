using Application.Features.Sales.Vendedores.Commands;
using Application.Features.Sales.Vendedores.Queries;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Sales;

public class VendedorController : MainController
{
    private readonly ILogger<VendedorController> _logger;

    public VendedorController(ILogger<VendedorController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetVendedoresFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetVendedoresQuery()));
    }

    [HttpGet("PorUsuario/{idUsuario}")]
    public async Task<IActionResult> GetByUsuario(long idUsuario)
    {
        return Ok(await Mediator.Send(new GetVendedorByUsuarioQuery { IdUsuario = idUsuario }));
    }

    [HttpGet("{idVend}")]
    public async Task<IActionResult> GetById(long idVend)
    {
        return Ok(await Mediator.Send(new GetVendedorByIdQuery { Id = idVend }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(VendedorDTO vendedorDTO)
    {
        return Ok(await Mediator.Send(new CreateVendedorCommand { VendedorDTO = vendedorDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(VendedorDTO vendedorDTO)
    {
        return Ok(await Mediator.Send(new UpdateVendedorCommand { VendedorDTO = vendedorDTO }));
    }

    [HttpDelete("Delete/{idVend}")]
    public async Task<IActionResult> Delete(long idVend)
    {
        return Ok(await Mediator.Send(new DeleteVendedorCommand { VendedorId = idVend }));
    }
}
