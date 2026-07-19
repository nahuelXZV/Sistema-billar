using Domain.DTOs.Sales;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;
using static Domain.Constants.Constantes;

namespace WebClient.Components.Sales.TurnoCaja;

public partial class TurnoCajaCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public IValidator<TurnoCajaDTO> Validator { get; set; } = default!;
    [Parameter] public TurnoCajaDTO TurnoCaja { get; set; } = new();
    [Parameter] public decimal MontoVendidoVendedor { get; set; }
    [Parameter] public bool EsSuperAdministrador { get; set; }
    [Parameter] public bool EsCierreCaja { get; set; }
    [Parameter] public List<VendedorDTO> ListaVendedores { get; set; } = [];
    [Parameter] public List<MetodoPagoDTO> ListaMetodosPago { get; set; } = [];

    public bool IsEditing => TurnoCaja.Id > 0;
    public bool IsClosed => TurnoCaja.Estado == (short)EstadoTurnoCaja.Cerrado && TurnoCaja.FechaCierre.HasValue;
    public bool SeEstaCerrando => TurnoCaja.Estado == (short)EstadoTurnoCaja.Cerrado;
    private string TituloFormulario => EsCierreCaja
        ? "Cerrar turno de caja"
        : IsEditing ? "Editar turno de caja" : "Nuevo turno de caja";

    private EditContext? _editContext;
    private DotNetObjectReference<TurnoCajaCreateComponent>? _objectHelper;
    private FluentValidationValidator<TurnoCajaDTO>? _fvValidator;

    protected override void OnInitialized()
    {
        TurnoCaja ??= new TurnoCajaDTO();
        TurnoCaja.Detalles ??= [];

        if (TurnoCaja.Estado == 0)
            TurnoCaja.Estado = (short)EstadoTurnoCaja.Abierto;

        CompletarMetodosPago();

        _editContext = new EditContext(TurnoCaja);
        _fvValidator = new FluentValidationValidator<TurnoCajaDTO>(_editContext, Validator);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _objectHelper = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("TurnoCajaCreateComponent.init", _objectHelper);
    }

    [JSInvokable]
    public async Task Validar()
    {
        if (IsClosed)
        {
            await ShowErrorMessage("No se puede editar un turno de caja cerrado.");
            return;
        }

        if (_editContext?.Validate() ?? false)
            await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            if (IsEditing)
                await AppServices.TurnoCajaService.Update(TurnoCaja);
            else
                await AppServices.TurnoCajaService.Create(TurnoCaja);

            await ShowSuccessMessage("Turno de caja guardado correctamente.");
            await Task.Delay(800);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}TurnoCaja/Listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }

    private void CompletarMetodosPago()
    {
        foreach (var detalle in TurnoCaja.Detalles)
        {
            detalle.MetodoPago ??= ListaMetodosPago.FirstOrDefault(metodo => metodo.Id == detalle.IdMetodoPago);
        }

        if (IsEditing) return;

        foreach (var metodoPago in ListaMetodosPago)
        {
            if (TurnoCaja.Detalles.Any(detalle => detalle.IdMetodoPago == metodoPago.Id)) continue;

            TurnoCaja.Detalles.Add(new TurnoCajaDetalleDTO
            {
                IdMetodoPago = metodoPago.Id,
                MontoApertura = 0,
                MontoVentasSistema = 0,
                MetodoPago = metodoPago
            });
        }
    }

    private string ObtenerNombreMetodoPago(long idMetodoPago)
    {
        return ListaMetodosPago.FirstOrDefault(metodo => metodo.Id == idMetodoPago)?.Nombre
            ?? $"Método {idMetodoPago}";
    }

    private string ObtenerNombreVendedor()
    {
        return TurnoCaja.Vendedor?.Nombre
            ?? ListaVendedores.FirstOrDefault(vendedor => vendedor.Id == TurnoCaja.IdVendedor)?.Nombre
            ?? "Sin vendedor asignado";
    }

    private static decimal? CalcularDiferencia(TurnoCajaDetalleDTO detalle)
    {
        if (!detalle.MontoCierreDeclarado.HasValue) return null;

        return detalle.MontoCierreDeclarado.Value - (detalle.MontoApertura + (detalle.MontoVentasSistema ?? 0));
    }

    private static string FormatearMonto(decimal? monto)
    {
        return monto.HasValue ? monto.Value.ToString("N2") : "-";
    }
}
