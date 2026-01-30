using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebClient.Common.Validation;
using Domain.DTOs.Inventory;

namespace WebClient.Components.Inventory.Almacen;

public partial class AlmacenCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<AlmacenDTO> Validator { get; set; }
    [Parameter] public AlmacenDTO Almacen { get; set; }
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<AlmacenCreateComponent>? _objectHelper;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Almacen);
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
            await JSRuntime.InvokeVoidAsync("AlmacenCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(AlmacenCreateComponent)}");
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

            if (Almacen.Id != 0)
            {
                var respuesta = await AppServices.AlmacenService.Update(Almacen);
            }
            else
            {
                var respuesta = await AppServices.AlmacenService.Create(Almacen);
            }

            await ShowSuccessMessage("Almacen guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Almacen/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
