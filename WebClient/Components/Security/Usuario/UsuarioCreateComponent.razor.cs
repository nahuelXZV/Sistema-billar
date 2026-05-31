using Domain.DTOs.Security;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Security.Usuario;

public partial class UsuarioCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Parameter] public UsuarioDTO Usuario { get; set; }
    [Parameter] public List<PerfilDTO> ListaPerfiles { get; set; }
    [Inject] public IValidator<UsuarioDTO> Validator { get; set; }
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<UsuarioCreateComponent>? _objectHelper;
    public bool ModificarContrasena { get; set; } = false;
    private bool IsEditing => Usuario?.Id > 0;
    private bool ShowPasswordField => ModificarContrasena || !IsEditing;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Usuario);
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
            await JSRuntime.InvokeVoidAsync("UsuarioCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(UsuarioCreateComponent)}");
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
            if (!ModificarContrasena && IsEditing) Usuario.Password = string.Empty;

            if (IsEditing)
            {
                await AppServices.UsuarioService.Update(Usuario);
            }
            else
            {
                await AppServices.UsuarioService.Create(Usuario);
            }

            await ShowSuccessMessage("Usuario guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Usuario/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
