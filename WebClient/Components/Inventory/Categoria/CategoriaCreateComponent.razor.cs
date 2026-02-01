using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace WebClient.Components.Inventory.Categoria;

public partial class CategoriaCreateComponent
{
    [Inject] public IValidator<CategoriaDTO> Validator { get; set; }
    [Parameter] public List<CategoriaDTO> ListaCategorias { get; set; } = new();
    [Parameter] public CategoriaDTO Categoria { get; set; } = new();
    [Parameter] public EventCallback OnCategoriaCreated { get; set; }
    private EditContext? _editContext { get; set; }

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Categoria);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
    }

    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Refresh()
    {
        Categoria = new CategoriaDTO();
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

            await Refresh();
            await ShowSuccessMessage("Categoria guardado correctamente");
            await OnCategoriaCreated.InvokeAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }
}
