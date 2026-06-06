using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;
using WebClient.Extensions;
using WebClient.Services;
using Domain.DTOs.Sales;
using System.Threading.Tasks;

namespace WebClient.Components.Sales.Venta;

public partial class VentaComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();
    private bool MostrarModalPago { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Model.PuntoVenta = await LoadPuntoVentaAsync();
    }

    private async Task<PuntoVentaViewModel> LoadPuntoVentaAsync()
    {
        var categoriasBase = await AppServices.CategoriaService.GetCategoriasBase();
        return PuntoVentaUtils.Create(categoriasBase, Model.Vendedor);
    }


    #region Venta Confirmada
    private async Task PagoConfirmado(IReadOnlyList<ItemsViewModel> paidItems)
    {
        try
        {
            var ventaDto = Model.PuntoVenta.GenerarDTOVenta();
            var response = await AppServices.VentaService.Create(ventaDto);

            foreach (var paidItem in paidItems)
            {
                var orderItem = Model.PuntoVenta.DetalleItems.FirstOrDefault(item => item.IdProducto == paidItem.IdProducto);
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

            await ShowSuccessMessage("Venta finalizada.");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }


    }
    #endregion

    #region Event Handlers
    private void AgregarItem(ProductosViewModel product)
    {
        var existingItem = Model.PuntoVenta.DetalleItems.FirstOrDefault(item => item.IdProducto == product.Id);
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
        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(orderItem => orderItem.IdProducto == productId);
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

    #region utils
    private void LimpiarVenta()
    {
        Model.PuntoVenta.DetalleItems.Clear();
        Model.PuntoVenta.SelectedPath.Clear();
        Model.PuntoVenta.CurrentNode = null;
        Model.PuntoVenta.ClienteSeleccionado = null;
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

        var item = Model.PuntoVenta.DetalleItems.FirstOrDefault(orderItem => orderItem.IdProducto == productId);
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
