using Application.Features.Contact.Proveedores.Commands;
using Application.Features.Contact.Proveedores.Queries;
using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Contact;

public class ProveedorController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetProveedoresFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetProveedoresQuery()));
    }

    [HttpGet("{idProveedor}")]
    public async Task<IActionResult> GetById(long idProveedor)
    {
        return Ok(await Mediator.Send(new GetProveedorByIdQuery { Id = idProveedor }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProveedorDTO proveedorDTO)
    {
        return Ok(await Mediator.Send(new CreateProveedorCommand { ProveedorDTO = proveedorDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProveedorDTO proveedorDTO)
    {
        return Ok(await Mediator.Send(new UpdateProveedorCommand { ProveedorDTO = proveedorDTO }));
    }

    [HttpDelete("Delete/{idProveedor}")]
    public async Task<IActionResult> Delete(long idProveedor)
    {
        return Ok(await Mediator.Send(new DeleteProveedorCommand { ProveedorId = idProveedor }));
    }
}
