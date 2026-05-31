using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Configuration.TipoMesa;

public partial class TipoMesaCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<TipoMesaDTO> Validator { get; set; }
    [Parameter] public TipoMesaDTO TipoMesa { get; set; }
    [Parameter] public List<ProductoDTO> ListaProductos { get; set; }
    public bool IsEditing => TipoMesa?.Id > 0;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<TipoMesaCreateComponent>? _objectHelper;
    private FluentValidationValidator<TipoMesaDTO> _fvValidator;

    protected override void OnInitialized()
    {
        TipoMesa ??= new TipoMesaDTO();
        _editContext = new EditContext(TipoMesa);
        _fvValidator = new FluentValidationValidator<TipoMesaDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("TipoMesaCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(TipoMesaCreateComponent)}");
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

            if (TipoMesa.Id != 0)
            {
                var respuesta = await AppServices.TipoMesaService.Update(TipoMesa);
            }
            else
            {
                var respuesta = await AppServices.TipoMesaService.Create(TipoMesa);
            }

            await ShowSuccessMessage("Tipo de mesa guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}TipoMesa/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
