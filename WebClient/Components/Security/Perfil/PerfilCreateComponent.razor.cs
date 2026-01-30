using Domain.DTOs.Security;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Security.Perfil;

public partial class PerfilCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Parameter] public PerfilDTO Perfil { get; set; }
    [Parameter] public List<ModuloDTO> ListaModulos { get; set; }
    [Inject] public IValidator<PerfilDTO> Validator { get; set; }
    private EditContext _editContext;
    private FluentValidationValidator<PerfilDTO> _fvValidator;
    private DotNetObjectReference<PerfilCreateComponent> _objectHelper;
    private List<long> ListaAccesosSeleccionados { get; set; } = new();

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Perfil);
        _fvValidator = new FluentValidationValidator<PerfilDTO>(_editContext, Validator);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Perfil.Id != 0) await ModoEdicion();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await InicializarJSHelper();
    }

    private async Task ModoEdicion()
    {
        ListaAccesosSeleccionados = Perfil.ListaAccesos.Select(a => a.AccesoId).ToList();
    }

    private async Task InicializarJSHelper()
    {
        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("PerfilCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(PerfilCreateComponent)}");
        }
    }

    private void OnAccesoSeleccionChanged(long accesoId, bool isChecked)
    {
        if (isChecked)
        {
            if (!ListaAccesosSeleccionados.Contains(accesoId))
                ListaAccesosSeleccionados.Add(accesoId);
        }
        else
        {
            if (ListaAccesosSeleccionados.Contains(accesoId))
                ListaAccesosSeleccionados.Remove(accesoId);
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        if (_editContext.Validate()) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            Perfil.ListaAccesos = ListaAccesosSeleccionados.Select(Id =>
                                    new PerfilAccesoDTO()
                                    {
                                        AccesoId = Id,
                                        PerfilId = Perfil.Id
                                    }).ToList();

            if (Perfil.Id != 0)
            {
                var respuesta = await AppServices.PerfilService.Update(Perfil);
            }
            else
            {
                var respuesta = await AppServices.PerfilService.Create(Perfil);
            }

            await ShowSuccessMessage("Perfil guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Perfil/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
