using Domain.DTOs.Sales;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Sales.MetodoPago;

public partial class MetodoPagoCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<MetodoPagoDTO> Validator { get; set; }
    [Parameter] public MetodoPagoDTO MetodoPago { get; set; }
    public bool IsEditing => MetodoPago?.Id > 0;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<MetodoPagoCreateComponent>? _objectHelper;
    private FluentValidationValidator<MetodoPagoDTO> _fvValidator;

    protected override void OnInitialized()
    {
        MetodoPago ??= new MetodoPagoDTO();
        _editContext = new EditContext(MetodoPago);
        _fvValidator = new FluentValidationValidator<MetodoPagoDTO>(_editContext, Validator);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await InicializarJSHelper();
    }

    private async Task InicializarJSHelper()
    {
        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("MetodoPagoCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(MetodoPagoCreateComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            if (MetodoPago.Id != 0)
            {
                var respuesta = await AppServices.MetodoPagoService.Update(MetodoPago);
            }
            else
            {
                var respuesta = await AppServices.MetodoPagoService.Create(MetodoPago);
            }

            await ShowSuccessMessage("Metodo de pago guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}MetodoPago/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
