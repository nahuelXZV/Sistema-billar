using Domain.DTOs.Sales;
using Domain.DTOs.Security;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Sales.Vendedor;

public partial class VendedorCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<VendedorDTO> Validator { get; set; }
    [Parameter] public VendedorDTO Vendedor { get; set; }
    [Parameter] public List<UsuarioDTO> ListaUsuarios { get; set; } = new();
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<VendedorCreateComponent>? _objectHelper;
    private FluentValidationValidator<VendedorDTO> _fvValidator;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Vendedor);
        _fvValidator = new FluentValidationValidator<VendedorDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("VendedorCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(VendedorCreateComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private string GetUsuarioText(UsuarioDTO usuario)
    {
        return $"{usuario.Username} - {usuario.Nombre} {usuario.Apellido}";
    }

    private long? GetUsuarioValue(UsuarioDTO usuario)
    {
        return usuario.Id;
    }

    private async Task Guardar()
    {
        try
        {
            if (Vendedor.Id != 0)
            {
                var respuesta = await AppServices.VendedorService.Update(Vendedor);
            }
            else
            {
                var respuesta = await AppServices.VendedorService.Create(Vendedor);
            }

            await ShowSuccessMessage("Vendedor guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Vendedor/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
