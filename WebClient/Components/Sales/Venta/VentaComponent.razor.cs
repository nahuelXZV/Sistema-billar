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
    private bool MostrarConfirmacionLimpiar { get; set; }
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
            ClienteDefault = Model.PuntoVenta.ClienteSeleccionado
                ?? await AppServices.ClienteService.GetById(AdminConfig.Personalizaciones.IdClienteDefault);

            if (Model.PuntoVenta.Clientes.Count == 0)
            {
                Model.PuntoVenta.Clientes = await AppServices.ClienteService.GetAll();
            }

            Model.PuntoVenta.IdClienteDefault = ClienteDefault.Id;
        }
    }

    private async Task<PuntoVentaViewModel> LoadPuntoVentaAsync()
    {
        var categoriasBase = await AppServices.CategoriaService.GetCategoriasBase();
        ClienteDefault = await AppServices.ClienteService.GetById(AdminConfig.Personalizaciones.IdClienteDefault);

        var puntoVenta = PuntoVentaUtils.Create(categoriasBase, Model.Vendedor);
        puntoVenta.ClienteSeleccionado = ClienteDefault;
        puntoVenta.IdClienteDefault = ClienteDefault.Id;
        puntoVenta.Clientes = await AppServices.ClienteService.GetAll();
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
                var orderItem = paidItem.IdOrdenVentaDetalle.HasValue
                    ? Model.PuntoVenta.DetalleItems.FirstOrDefault(item => item.IdOrdenVentaDetalle == paidItem.IdOrdenVentaDetalle)
                    : Model.PuntoVenta.DetalleItems.FirstOrDefault(item =>
                        item.IdProducto == paidItem.IdProducto &&
                        item.IdProductoConversion == paidItem.IdProductoConversion &&
                        item.IdCliente == paidItem.IdCliente &&
                        item.EsTiempoMesa == paidItem.EsTiempoMesa);
                if (orderItem is null)
                {
                    continue;
                }

                orderItem.Cantidad = (orderItem.Cantidad - paidItem.Cantidad).Redondear();
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

    private void AgregarItem(ProductoSeleccionado seleccion)
    {
        var product = seleccion.Producto;
        var precioUnidad = seleccion.PrecioUnidad;
        var existingItem = Model.PuntoVenta.DetalleItems.FirstOrDefault(item =>
            item.IdProducto == product.Id &&
            item.IdProductoConversion == precioUnidad.IdProductoConversion &&
            item.IdCliente == Model.PuntoVenta.IdClienteDefault &&
            !item.EsTiempoMesa);
        if (existingItem is not null)
        {
            existingItem.Cantidad = (existingItem.Cantidad + 1).Redondear();
            return;
        }

        Model.PuntoVenta.DetalleItems.Add(new ItemsViewModel
        {
            IdCliente = Model.PuntoVenta.IdClienteDefault,
            IdProducto = product.Id,
            IdProductoConversion = precioUnidad.IdProductoConversion,
            Nombre = product.Nombre,
            NombreUnidadMedida = precioUnidad.NombreUnidadMedida,
            AbreviaturaUnidadMedida = precioUnidad.AbreviaturaUnidadMedida,
            FactorConversion = precioUnidad.FactorConversion,
            Cantidad = 1,
            PrecioUnitario = precioUnidad.Precio,
        });
    }

    private void EliminarItem(ItemsViewModel itemSeleccionado)
    {
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(
            orderItem => MismoDetalle(orderItem, itemSeleccionado));
        if (item is not null)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }

    private void IncrementarCantidad(ItemsViewModel item)
    {
        CambiarCantidad(item, 1);
    }

    private void ReducirCantidad(ItemsViewModel item)
    {
        CambiarCantidad(item, -1);
    }

    private void SetearCantidad(CantidadModificada quantityChange)
    {
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(orderItem =>
            orderItem.IdProducto == quantityChange.ProductId &&
            orderItem.IdProductoConversion == quantityChange.ProductConversionId &&
            orderItem.IdCliente == quantityChange.IdCliente &&
            orderItem.EsTiempoMesa == quantityChange.EsTiempoMesa);
        if (item is null)
        {
            return;
        }

        item.Cantidad = quantityChange.Cantidad.Redondear();
        if (item.Cantidad <= 0)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }
    #endregion

    #region Utils
    private void SolicitarLimpiarVenta()
    {
        MostrarConfirmacionLimpiar = true;
    }

    private void CancelarLimpiarVenta()
    {
        MostrarConfirmacionLimpiar = false;
    }

    private void ConfirmarLimpiarVenta()
    {
        MostrarConfirmacionLimpiar = false;
        LimpiarVenta();
    }

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
    private void CambiarCantidad(ItemsViewModel itemSeleccionado, decimal delta)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(
            orderItem => MismoDetalle(orderItem, itemSeleccionado));
        if (item is null)
        {
            return;
        }

        item.Cantidad = (item.Cantidad + delta).Redondear();
        if (item.Cantidad <= 0)
        {
            Model.PuntoVenta.DetalleItems.Remove(item);
        }
    }

    private void CambiarClienteDetalle(ItemsViewModel itemSeleccionado)
    {
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(item => ReferenceEquals(item, itemSeleccionado));
        if (item is null || !item.IdCliente.HasValue)
        {
            return;
        }

        var itemConMismoCliente = Model.PuntoVenta.DetalleItems.FirstOrDefault(itemActual =>
            !ReferenceEquals(itemActual, item) &&
            itemActual.IdProducto == item.IdProducto &&
            itemActual.IdProductoConversion == item.IdProductoConversion &&
            itemActual.IdCliente == item.IdCliente &&
            itemActual.EsTiempoMesa == item.EsTiempoMesa);

        if (itemConMismoCliente is null)
        {
            return;
        }

        itemConMismoCliente.Cantidad = (itemConMismoCliente.Cantidad + item.Cantidad).Redondear();
        Model.PuntoVenta.DetalleItems.Remove(item);
    }

    private static bool MismoDetalle(ItemsViewModel item, ItemsViewModel seleccionado) =>
        item.IdProducto == seleccionado.IdProducto &&
        item.IdProductoConversion == seleccionado.IdProductoConversion &&
        item.IdCliente == seleccionado.IdCliente &&
        item.EsTiempoMesa == seleccionado.EsTiempoMesa;
    #endregion
}
