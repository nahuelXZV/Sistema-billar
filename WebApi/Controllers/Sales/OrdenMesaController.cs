using Application.Features.Sales.OrdenMesas.Commands;
using Application.Features.Sales.OrdenMesas.Queries;
using Domain.DTOs.Sales;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Sales;

public class OrdenMesaController : MainController
{
    [HttpGet("Abiertas")]
    public async Task<IActionResult> GetAbiertas()
    {
        return Ok(await Mediator.Send(new GetOrdenesMesaAbiertasQuery()));
    }

    [HttpGet("Mesa/{idMesa}")]
    public async Task<IActionResult> GetByMesa(long idMesa)
    {
        return Ok(await Mediator.Send(new GetOrdenMesaByMesaQuery { IdMesa = idMesa }));
    }

    [HttpPost("Guardar")]
    public async Task<IActionResult> Guardar(OrdenMesaDTO ordenMesa)
    {
        return Ok(await Mediator.Send(new GuardarOrdenMesaCommand { OrdenMesa = ordenMesa }));
    }

    [HttpPost("IniciarCronometro/{idOrdenVenta}")]
    public async Task<IActionResult> IniciarCronometro(long idOrdenVenta)
    {
        return Ok(await Mediator.Send(new IniciarCronometroMesaCommand
        {
            IdOrdenVenta = idOrdenVenta
        }));
    }

    [HttpPost("FinalizarCronometro/{idOrdenVenta}")]
    public async Task<IActionResult> FinalizarCronometro(long idOrdenVenta)
    {
        return Ok(await Mediator.Send(new FinalizarCronometroMesaCommand
        {
            IdOrdenVenta = idOrdenVenta
        }));
    }
}
