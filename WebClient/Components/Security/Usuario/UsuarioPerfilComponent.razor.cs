using Domain.DTOs.Security;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Security.Usuario;

public partial class UsuarioPerfilComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<UsuarioPerfilDTO> Validator { get; set; }
    [Parameter] public UsuarioPerfilDTO Usuario { get; set; } = new();
    public bool ModificarContrasena { get; set; }

    private EditContext? _editContext { get; set; }
    private FluentValidationValidator<UsuarioPerfilDTO> _fvValidator;
    private DotNetObjectReference<UsuarioPerfilComponent>? _objectHelper;
    private string _usernameOriginal = string.Empty;
    private bool UsernameModificado => !string.Equals(Usuario.Username, _usernameOriginal, StringComparison.Ordinal);
    private bool RequierePasswordActual => ModificarContrasena || UsernameModificado;

    protected override void OnInitialized()
    {
        _usernameOriginal = Usuario.Username;
        _editContext = new EditContext(Usuario);
        _fvValidator = new FluentValidationValidator<UsuarioPerfilDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("UsuarioPerfilComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(UsuarioPerfilComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        Usuario.ModificarContrasena = ModificarContrasena;
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            Usuario.ModificarContrasena = ModificarContrasena;

            if (RequierePasswordActual && string.IsNullOrWhiteSpace(Usuario.PasswordActual))
            {
                await ShowErrorMessage("Ingrese la contrasena actual para confirmar el cambio.");
                return;
            }

            if (!ModificarContrasena && !UsernameModificado)
            {
                Usuario.PasswordActual = string.Empty;
            }

            if (!ModificarContrasena)
            {
                Usuario.NuevaPassword = string.Empty;
                Usuario.ConfirmarPassword = string.Empty;
            }

            await AppServices.UsuarioService.UpdatePerfil(Usuario);
            await ShowSuccessMessage("Perfil actualizado correctamente");
            await Task.Delay(800);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Usuario/RefrescarPerfil", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
