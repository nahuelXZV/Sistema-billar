using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;
using static Domain.Constants.Constantes;

namespace WebClient.Controllers.Sales;

public class TurnoCajaController : MainController
{
    public TurnoCajaController(ViewModelFactory viewModelFactory, IAppServices services)
        : base(viewModelFactory, services)
    {
    }

    [HttpGet]
    public IActionResult Listado()
    {
        var model = _viewModelFactory.Create<TurnoCajaViewModel>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var turnosCaja = await _appServices.TurnoCajaService.GetAll(new FilterDTO
            {
                Search = search,
                Limit = limit,
                Offset = offset
            });

            return Ok(turnosCaja);
        }
        catch
        {
            return Ok(new ResponseFilterDTO<TurnoCajaDTO> { Total = 0, Data = [] });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] long id = 0)
    {
        return await CargarFormulario(id, false);
    }

    [HttpPost]
    public async Task<IActionResult> Cerrar([FromForm] long id)
    {
        if (id <= 0)
        {
            this.AddErrorTempMessage("Debe seleccionar un turno de caja.");
            return RedirectToAction(nameof(Listado));
        }

        return await CargarFormulario(id, true);
    }

    private async Task<IActionResult> CargarFormulario(long id, bool esCierreCaja)
    {
        var model = _viewModelFactory.Create<TurnoCajaViewModel>();
        model.IncluirBlazorComponents = true;
        model.EsCierreCaja = esCierreCaja;

        if (model.EsSuperAdministrador)
        {
            model.ListaVendedores = await _appServices.VendedorService.GetAll();
        }
        else
        {
            var vendedorUsuario = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);
            model.ListaVendedores = vendedorUsuario.Id > 0 ? [vendedorUsuario] : [];
        }

        model.ListaMetodosPago = (await _appServices.MetodoPagoService.GetAll())
            .Where(metodo => metodo.Activo && !metodo.Eliminado)
            .ToList();

        if (id > 0)
        {
            model.TurnoCaja = await _appServices.TurnoCajaService.GetById(id);

            if (esCierreCaja && model.TurnoCaja.Estado == (short)EstadoTurnoCaja.Cerrado)
            {
                this.AddErrorTempMessage("El turno de caja ya está cerrado.");
                return RedirectToAction(nameof(Listado));
            }

            model.TurnoCaja.Estado = esCierreCaja ? (short)EstadoTurnoCaja.Cerrado : model.TurnoCaja.Estado;

            model.MontoVendidoVendedor = await _appServices.VentaService.GetMontoVendidoVendedor(model.TurnoCaja.IdVendedor, model.TurnoCaja.Id);
            var montosPorMetodoPago = await _appServices.VentaService.GetMontosVendidosPorMetodoPago(model.TurnoCaja.IdVendedor, model.TurnoCaja.Id);

            foreach (var detalle in model.TurnoCaja.Detalles)
            {
                detalle.MontoVentasSistema = montosPorMetodoPago.FirstOrDefault(monto => monto.IdMetodoPago == detalle.IdMetodoPago)?.MontoVendido ?? 0;
            }
        }
        else
        {
            model.TurnoCaja = new TurnoCajaDTO
            {
                IdVendedor = model.EsSuperAdministrador ? 0 : model.ListaVendedores.FirstOrDefault()?.Id ?? 0,
                Vendedor = model.EsSuperAdministrador ? null : model.ListaVendedores.FirstOrDefault(),
                FechaApertura = DateTime.Now,
                Estado = (short)EstadoTurnoCaja.Abierto,
                Detalles = model.ListaMetodosPago.Select(metodo => new TurnoCajaDetalleDTO
                {
                    IdMetodoPago = metodo.Id,
                    MontoApertura = 0,
                    MontoVentasSistema = 0,
                    MetodoPago = metodo
                }).ToList()
            };
        }

        return View("Crear", model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        var esSuperAdministrador = bool.TryParse(
            User.GetClaimValue(WebClient.Common.Constantes.ClaimTypes.EsSuperAdmin),
            out var resultado) && resultado;

        if (!esSuperAdministrador)
        {
            this.AddErrorTempMessage("Solo un superadministrador puede eliminar turnos de caja.");
            return RedirectToAction(nameof(Listado));
        }

        try
        {
            await _appServices.TurnoCajaService.Delete(id);
            this.AddSuccessTempMessage("Turno de caja eliminado correctamente.");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }

        return RedirectToAction(nameof(Listado));
    }
}
