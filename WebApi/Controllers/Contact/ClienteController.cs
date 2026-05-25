using Application.Features.Contact.Clientes.Commands;
using Application.Features.Contact.Clientes.Queries;
using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Contact;

public class ClienteController : MainController
{
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(ILogger<ClienteController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetClientesFilterQuery { Filter = filter }));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetClientesQuery()));
    }

    [HttpGet("{idCli}")]
    public async Task<IActionResult> GetById(long idCli)
    {
        return Ok(await Mediator.Send(new GetClienteByIdQuery { Id = idCli }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ClienteDTO clienteDTO)
    {
        return Ok(await Mediator.Send(new CreateClienteCommand { ClienteDTO = clienteDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ClienteDTO clienteDTO)
    {
        return Ok(await Mediator.Send(new UpdateClienteCommand { ClienteDTO = clienteDTO }));
    }

    [HttpDelete("Delete/{idCli}")]
    public async Task<IActionResult> Delete(long idCli)
    {
        return Ok(await Mediator.Send(new DeleteClienteCommand { ClienteId = idCli }));
    }
}
