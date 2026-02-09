using System.Threading.Tasks;
using Domain.DTOs.Inventory;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebClient.Components.Inventory.Almacen;

namespace WebClient.Components.Inventory.Categoria;

public partial class CategoriaListadoComponent
{
    [Parameter] public List<CategoriaDTO> ListaCategorias { get; set; } = new();
    public CategoriaDTO CategoriaSelected { get; set; } = new();
    private DotNetObjectReference<CategoriaListadoComponent>? _objectHelper;
    public List<CategoriaDTO> ListaCategoriasSinNivel { get; set; } = new();
    public CategoriaCreateComponent categoriaCreateComponent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        ListaCategoriasSinNivel = await AppServices.CategoriaService.GetAllSinNivel();
        StateHasChanged();
        await InicializarJSHelper();
    }

    private async Task InicializarJSHelper()
    {
        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("CategoriaListadoComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(CategoriaListadoComponent)}");
        }
    }

    public void CategoriaSeleccionado(CategoriaDTO categoria)
    {
        CategoriaSelected.IdCategoriaPadre = categoria.IdCategoriaPadre;
        CategoriaSelected.Activo = categoria.Activo;
        CategoriaSelected.Descripcion = categoria.Descripcion;
        CategoriaSelected.ImagenUrl = categoria.ImagenUrl;
        CategoriaSelected.Nombre = categoria.Nombre;
        CategoriaSelected.OrdenVisual = categoria.OrdenVisual;
        CategoriaSelected.Id = categoria.Id;
    }

    [JSInvokable]
    public async Task Crear()
    {
        if (categoriaCreateComponent != null) await categoriaCreateComponent.Refresh();
        await InvokeAsync(StateHasChanged);
    }

    public async Task Refresh()
    {
        ListaCategorias = await AppServices.CategoriaService.GetAll();
        StateHasChanged();
    }

}
