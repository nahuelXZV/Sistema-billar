using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.JSInterop;
using WebClient.Configs;
using WebClient.Exceptions;
using WebClient.Services;

namespace WebClient.Components;

public partial class MainBaseComponent : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [CascadingParameter] public Task<AuthenticationState> AuthenticationState { get; set; }
    [CascadingParameter] protected AdminConfig AdminConfig { get; set; }
    [CascadingParameter] public IAppServices AppServices { get; set; }
    protected ClaimsPrincipal User { get; set; } = new ClaimsPrincipal();

    public bool _modalAlertaVisible { get; set; }
    public string _modalAlertaTitulo { get; set; } = "Alerta";
    public string _modalAlertaContenido { get; set; } = "Alerta";
    public string _modalAlertaTipo { get; set; } = "info";

    private async Task<ClaimsPrincipal> GetAuth()
    {
        if (AuthenticationState is null) return null;

        var authState = await AuthenticationState;
        var user = authState?.User;

        return user;
    }

    protected bool IsAuthenticated()
    {
        return User?.Identity is not null && User.Identity.IsAuthenticated;
    }

    protected override async Task OnInitializedAsync()
    {
        User = await GetAuth();
    }

    public async Task ShowSuccessMessage(string message)
    {
        await JSRuntime.InvokeVoidAsync("window.showSuccessMessage", message);
    }

    public async Task ShowInfoMessage(string message)
    {
        await JSRuntime.InvokeVoidAsync("window.ShowInfoMessage", message);
    }

    public async Task ShowWarnMessage(string message)
    {
        await JSRuntime.InvokeVoidAsync("window.ShowWarnMessage", message);
    }

    public async Task ShowErrorMessage(string message)
    {
        await JSRuntime.InvokeVoidAsync("window.showErrorMessage", message);
    }

    public async Task ShowErrorMessage(Exception ex)
    {
        if (ex is ApiResponseException apiException)
        {
            await JSRuntime.InvokeVoidAsync(
                "window.showDetailedErrorMessage",
                apiException.Message,
                apiException.Error?.DiagnosticMessage,
                apiException.Error?.ServiceStackTrace,
                apiException.Error?.ErrorDetails);
            return;
        }

        await JSRuntime.InvokeVoidAsync(
            "window.showDetailedErrorMessage",
            ex.Message,
            ex.InnerException?.Message,
            ex.StackTrace,
            null);
    }
}
