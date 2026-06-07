using Domain.DTOs.Configuration;
using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class VentaMesasComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();

    private MesaDTO? SelectedMesa { get; set; }
    private VentaViewModel? SelectedVentaModel { get; set; }
    private int ActiveTablesCount => Model.Mesas.Count(mesa => mesa.Activo);
    private int InactiveTablesCount => Model.Mesas.Count(mesa => !mesa.Activo);
    private string SaleContextLabel => SelectedMesa is null ? "Venta unica" : "Mesa seleccionada";
    private string SaleContextTitle => SelectedMesa?.Nombre ?? "Venta directa";

    private void SelectMesa(MesaDTO mesa)
    {
        SelectedMesa = mesa;
        SelectedVentaModel = CreateVentaModel();
    }

    private void OpenDirectSale()
    {
        SelectedMesa = null;
        SelectedVentaModel = CreateVentaModel();
    }

    private VentaViewModel CreateVentaModel()
    {
        return new VentaViewModel
        {
            Vendedor = Model.Vendedor,
            PuntoVenta = Model.PuntoVenta,
        };
    }

    private void BackToTables()
    {
        SelectedMesa = null;
        SelectedVentaModel = null;
    }

    private void TiempoFinalizado()
    {
        StateHasChanged();
    }

    private static string GetMesaEstado(MesaDTO mesa)
    {
        return mesa.Activo ? "Libre" : "Inactiva";
    }

    private static string GetMesaAccion(MesaDTO mesa)
    {
        return mesa.Activo ? "Abrir venta" : "Ver mesa";
    }

}
