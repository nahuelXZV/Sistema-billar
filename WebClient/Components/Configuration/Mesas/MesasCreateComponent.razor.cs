using Domain.DTOs.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Configuration.Mesas;

public partial class MesasCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<MesaDTO> Validator { get; set; }
    [Parameter] public MesaDTO Mesa { get; set; }
    [Parameter] public List<TipoMesaDTO> ListaTipoMesas { get; set; }
    public bool IsEditing => Mesa?.Id > 0;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<MesasCreateComponent>? _objectHelper;
    private FluentValidationValidator<MesaDTO> _fvValidator;

    protected override void OnInitialized()
    {
        Mesa ??= new MesaDTO();
        _editContext = new EditContext(Mesa);
        _fvValidator = new FluentValidationValidator<MesaDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("MesasCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(MesasCreateComponent)}");
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

            if (Mesa.Id != 0)
            {
                var respuesta = await AppServices.MesasService.Update(Mesa);
            }
            else
            {
                var respuesta = await AppServices.MesasService.Create(Mesa);
            }

            await ShowSuccessMessage("Mesa guardada correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Mesas/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
