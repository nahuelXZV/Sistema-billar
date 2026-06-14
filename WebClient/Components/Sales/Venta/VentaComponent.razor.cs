using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;
using WebClient.Extensions;
using Domain.DTOs.Contact;
using static Domain.Constants.Constantes;
using WebClient.Common.Utils;

namespace WebClient.Components.Sales.Venta;

public partial class VentaComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();
    [Parameter] public bool EsVentaMesa { get; set; } = false;
    [Parameter] public bool GuardandoOrden { get; set; } = false;
    [Parameter] public EventCallback OnGuardarOrden { get; set; }
    [Parameter] public EventCallback OnPrepararPago { get; set; }
    [Parameter] public EventCallback<long> OnVentaFinalizada { get; set; }
    private ClienteDTO ClienteDefault { get; set; } = new();
    private bool MostrarModalPago { get; set; }
    private bool BloquearPagoTiempo => EsVentaMesa && Model.OrdenMesa?.EstadoUsoMesa == (short)EstadoUsoMesa.EnCurso;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Model.PuntoVenta is null)
        {
            Model.PuntoVenta = await LoadPuntoVentaAsync();
        }
        else
        {
            ClienteDefault = Model.PuntoVenta.ClienteSeleccionado!;
        }
    }

    private async Task<PuntoVentaViewModel> LoadPuntoVentaAsync()
    {
        var categoriasBase = await AppServices.CategoriaService.GetCategoriasBase();
        ClienteDefault = await AppServices.ClienteService.GetById(AdminConfig.Personalizaciones.IdClienteDefault);

        var puntoVenta = PuntoVentaUtils.Create(categoriasBase, Model.Vendedor);
        puntoVenta.ClienteSeleccionado = ClienteDefault;
        return puntoVenta;
    }

    #region Venta Confirmada
    private async Task PagoConfirmado(IReadOnlyList<ItemsViewModel> paidItems)
    {
        try
        {
            Model.PagoEnProceso = true;

            if (OnPrepararPago.HasDelegate)
            {
                await OnPrepararPago.InvokeAsync();
            }

            var ventaDto = Model.PuntoVenta.GenerarDTOVenta();
            var response = await AppServices.VentaService.Create(ventaDto);
            Model.PuntoVenta.IdempotencyKey = null;

            foreach (var paidItem in paidItems)
            {
                var orderItem = Model.PuntoVenta.DetalleItems.FirstOrDefault(item =>
                    item.IdProducto == paidItem.IdProducto &&
                    item.EsTiempoMesa == paidItem.EsTiempoMesa);
                if (orderItem is null)
                {
                    continue;
                }

                orderItem.Cantidad = orderItem.Cantidad - paidItem.Cantidad;
                orderItem.Cantidad.Redondear();
                if (orderItem.Cantidad <= 0)
                {
                    Model.PuntoVenta.DetalleItems.Remove(orderItem);
                }
            }

            if (Model.PuntoVenta.DetalleItems.Count == 0)
            {
                LimpiarVenta();
            }

            if (OnVentaFinalizada.HasDelegate)
            {
                await OnVentaFinalizada.InvokeAsync(response);
            }

            Model.PagoEnProceso = false;
            await ShowSuccessMessage(EsVentaMesa ? "Pago registrado." : "Venta finalizada.");
        }
        catch (Exception ex)
        {
            Model.PagoEnProceso = false;

            await ShowErrorMessage(ex);
            throw;
        }


    }
    #endregion

    #region Event Handlers
    private async Task GuardarOrdenAsync()
    {
        if (!OnGuardarOrden.HasDelegate)
        {
            return;
        }

        try
        {
            await OnGuardarOrden.InvokeAsync();
            await ShowSuccessMessage("Orden de mesa guardada.");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }
    }

    private void AgregarItem(Productos product)
    {
        var existingItem = Model.PuntoVenta.DetalleItems.FirstOrDefault(item =>
            item.IdProducto == product.Id && !item.EsTiempoMesa);
        if (existingItem is not null)
        {
            existingItem.Cantidad = existingItem.Cantidad + 1;
            existingItem.Cantidad.Redondear();
            return;
        }

        Model.PuntoVenta.DetalleItems.Add(new ItemsViewModel
        {
            IdProducto = product.Id,
            Nombre = product.Nombre,
            Cantidad = 1,
            PrecioUnitario = product.Precio,
        });
    }

    private void EliminarItem(long productId)
    {
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(
            orderItem => orderItem.IdProducto == productId && !orderItem.EsTiempoMesa);
        if (item is not null)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }

    private void IncrementarCantidad(long productId)
    {
        CambiarCantidad(productId, 1);
    }

    private void ReducirCantidad(long productId)
    {
        CambiarCantidad(productId, -1);
    }

    private void SetearCantidad(CantidadModificada quantityChange)
    {
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(orderItem => orderItem.IdProducto == quantityChange.ProductId);
        if (item is null)
        {
            return;
        }

        item.Cantidad = quantityChange.Cantidad;
        item.Cantidad.Redondear();
        if (item.Cantidad <= 0)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }
    #endregion

    #region Utils
    private void LimpiarVenta()
    {
        Model.PuntoVenta.DetalleItems.Clear();
        Model.PuntoVenta.SelectedPath.Clear();
        Model.PuntoVenta.CurrentNode = null;
        Model.PuntoVenta.ClienteSeleccionado = ClienteDefault;
        Model.PuntoVenta.NotaVenta = string.Empty;
        Model.PuntoVenta.DescuentoGlobal = 0;
        Model.PuntoVenta.RecargoGlobal = 0;
    }

    private void MostrarModalPagoHandler()
    {
        if (Model.PuntoVenta?.DetalleItems.Count > 0)
        {
            MostrarModalPago = true;
        }
    }
    private void CambiarCantidad(long productId, decimal delta)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(
            orderItem => orderItem.IdProducto == productId && !orderItem.EsTiempoMesa);
        if (item is null)
        {
            return;
        }

        item.Cantidad = item.Cantidad + delta;
        item.Cantidad.Redondear();
        if (item.Cantidad <= 0)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }
    #endregion
}
