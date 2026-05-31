using Domain.DTOs.Contact;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Contact.Cliente;

public partial class ClienteCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<ClienteDTO> Validator { get; set; }
    [Parameter] public ClienteDTO Cliente { get; set; }
    public bool IsEditing => Cliente?.Id > 0;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<ClienteCreateComponent>? _objectHelper;
    private FluentValidationValidator<ClienteDTO> _fvValidator;

    protected override void OnInitialized()
    {
        Cliente ??= new ClienteDTO();
        _editContext = new EditContext(Cliente);
        _fvValidator = new FluentValidationValidator<ClienteDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("ClienteCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(ClienteCreateComponent)}");
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
            if (Cliente.Id != 0)
            {
                var respuesta = await AppServices.ClienteService.Update(Cliente);
            }
            else
            {
                var respuesta = await AppServices.ClienteService.Create(Cliente);
            }

            await ShowSuccessMessage("Cliente guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Cliente/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
