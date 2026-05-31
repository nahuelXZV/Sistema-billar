using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebClient.Common.Validation;

namespace WebClient.Components.Inventory.Categoria;

public partial class CategoriaCreateComponent
{
    [Inject] public IValidator<CategoriaDTO> Validator { get; set; }
    [Parameter] public List<CategoriaDTO> ListaCategorias { get; set; } = new();
    [Parameter] public CategoriaDTO Categoria { get; set; } = new();
    [Parameter] public EventCallback OnCategoriaCreated { get; set; }
    private EditContext? _editContext { get; set; }
    private FluentValidationValidator<CategoriaDTO> _fvValidator;
    private bool IsEditing => Categoria?.Id > 0;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Categoria);
        _fvValidator = new FluentValidationValidator<CategoriaDTO>(_editContext, Validator);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
    }

    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    public async Task Refresh()
    {
        ResetCategoria();
        ListaCategorias = await AppServices.CategoriaService.GetAllSinNivel();
        StateHasChanged();
    }

    private async Task Guardar()
    {
        try
        {
            if (Categoria.Id != 0)
            {
                var respuesta = await AppServices.CategoriaService.Update(Categoria);
            }
            else
            {
                var respuesta = await AppServices.CategoriaService.Create(Categoria);
            }

            //await Refresh();
            await ShowSuccessMessage("Categoria guardado correctamente");
            await OnCategoriaCreated.InvokeAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }

    private async Task Eliminar()
    {
        try
        {
            if (Categoria.Id == 0) return;
            await AppServices.CategoriaService.Delete(Categoria.Id);
            //await ();
            await ShowSuccessMessage("Categoria eliminado correctamente");
            await OnCategoriaCreated.InvokeAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }

    private void ResetCategoria()
    {
        Categoria.Id = 0;
        Categoria.Nombre = string.Empty;
        Categoria.Descripcion = string.Empty;
        Categoria.ImagenUrl = string.Empty;
        Categoria.IdCategoriaPadre = null;
        Categoria.OrdenVisual = 0;
        Categoria.Activo = true;
    }

}
