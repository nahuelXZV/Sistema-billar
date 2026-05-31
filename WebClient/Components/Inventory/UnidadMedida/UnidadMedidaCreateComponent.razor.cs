using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebClient.Configs;
using Domain.DTOs.Shared;
using WebClient.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebClient.Components.Inventory.UnidadMedida;

public partial class UnidadMedidaCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<UnidadMedidaDTO> Validator { get; set; }
    [Parameter] public UnidadMedidaDTO Unidad { get; set; }
    public bool IsEditing => Unidad?.Id > 0;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<UnidadMedidaCreateComponent>? _objectHelper;
    private List<SelectOptionDTO<int>> ListaUnidades { get; set; }
    private FluentValidationValidator<UnidadMedidaDTO> _fvValidator;

    protected override void OnInitialized()
    {
        Unidad ??= new UnidadMedidaDTO();
        _editContext = new EditContext(Unidad);
        _fvValidator = new FluentValidationValidator<UnidadMedidaDTO>(_editContext, Validator);
        ListaUnidades = new List<SelectOptionDTO<int>>()

        {
            new SelectOptionDTO<int>() { Value = (int)Domain.Constants.Constantes.TipoUnidadMedida.Unidad, Label = "Unidad" },
            new SelectOptionDTO<int>() { Value = (int)Domain.Constants.Constantes.TipoUnidadMedida.Peso, Label = "Peso" },
            new SelectOptionDTO<int>() { Value = (int)Domain.Constants.Constantes.TipoUnidadMedida.Volumen, Label = "Volumen" },
            new SelectOptionDTO<int>() { Value = (int)Domain.Constants.Constantes.TipoUnidadMedida.Longitud, Label = "Longitud" },
            new SelectOptionDTO<int>() { Value = (int)Domain.Constants.Constantes.TipoUnidadMedida.Tiempo, Label = "Tiempo" },
        };
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
            await JSRuntime.InvokeVoidAsync("UnidadMedidaCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(UnidadMedidaCreateComponent)}");
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

            if (Unidad.Id != 0)
            {
                var respuesta = await AppServices.UnidadMedidaService.Update(Unidad);
            }
            else
            {
                var respuesta = await AppServices.UnidadMedidaService.Create(Unidad);
            }

            await ShowSuccessMessage("Unidad de medida guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}UnidadMedida/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
