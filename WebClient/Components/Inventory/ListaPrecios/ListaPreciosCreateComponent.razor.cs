using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Inventory.ListaPrecios;

public partial class ListaPreciosCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<ListaPrecioDTO> Validator { get; set; }
    [Parameter] public ListaPrecioDTO ListaPrecio { get; set; }
    [Parameter] public List<ProductoDTO> ListadoProductos { get; set; } = new();
    private bool IsEditing => ListaPrecio?.Id > 0;
    private FluentValidationValidator<ListaPrecioDTO> _fvValidator;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<ListaPreciosCreateComponent>? _objectHelper;
    private ListaPrecioDetalleDTO DetalleDTO { get; set; } = new();
    private List<ListaPrecioDetalleDTO> ListadoDetalles { get; set; } = new();
    private int PaginaActual = 1;
    private int RegistrosPorPagina = 5;
    private IEnumerable<ListaPrecioDetalleDTO> DetallesPaginados => ListadoDetalles
        .Skip((PaginaActual - 1) * RegistrosPorPagina)
        .Take(RegistrosPorPagina);
    private int TotalPaginas => ListadoDetalles.Count > 0 ? (int)Math.Ceiling(ListadoDetalles.Count / (double)RegistrosPorPagina) : 0;

    protected override void OnInitialized()
    {
        ListaPrecio ??= new ListaPrecioDTO();
        _editContext = new EditContext(ListaPrecio);
        _fvValidator = new FluentValidationValidator<ListaPrecioDTO>(_editContext, Validator);
    }

    protected override void OnParametersSet()
    {
        ListaPrecio ??= new ListaPrecioDTO();
        ListadoDetalles = new();

        foreach (var item in ListaPrecio?.ListaDetalles ?? new())
        {
            var producto = ListadoProductos.Where(p => p.Id == item.IdProducto).FirstOrDefault();
            if (producto != null)
            {
                ListadoDetalles.Add(new ListaPrecioDetalleDTO()
                {
                    NombreProducto = producto.Nombre,
                    IdProducto = item.IdProducto,
                    Precio = item.Precio,
                });
            }

        }
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
            await JSRuntime.InvokeVoidAsync("ListaPreciosCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(ListaPreciosCreateComponent)}");
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

            ListaPrecio.ListaDetalles = ListadoDetalles;
            if (ListaPrecio.Id != 0)
            {
                var respuesta = await AppServices.ListaPreciosService.Update(ListaPrecio);
            }
            else
            {
                var respuesta = await AppServices.ListaPreciosService.Create(ListaPrecio);
            }

            await ShowSuccessMessage("Lista de precios guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}ListaPrecios/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }

    private async Task AgregarDetalle()
    {
        if (DetalleDTO.IdProducto == 0 || DetalleDTO.Precio <= 0)
        {
            await ShowErrorMessage("Debe seleccionar un producto y asignar un precio mayor a cero.");
            return;
        }
        var producto = ListadoProductos.FirstOrDefault(p => p.Id == DetalleDTO.IdProducto);
        if (producto == null)
        {
            await ShowErrorMessage("Producto no encontrado.");
            return;
        }
        var detalle = ListadoDetalles.FirstOrDefault(d => d.IdProducto == producto.Id);
        if (detalle != null)
        {
            detalle.Precio = DetalleDTO.Precio;
        }
        else
        {
            DetalleDTO.NombreProducto = producto.Nombre;
            ListadoDetalles.Add(DetalleDTO);
        }
        DetalleDTO = new ListaPrecioDetalleDTO();
        StateHasChanged();
    }

    private async Task EditarDetalle(long id)
    {
        var detalle = ListadoDetalles.FirstOrDefault(d => d.IdProducto == id);
        if (detalle == null)
        {
            await ShowErrorMessage("Detalle no encontrado.");
            return;
        }
        DetalleDTO = new ListaPrecioDetalleDTO
        {
            IdProducto = detalle.IdProducto,
            NombreProducto = detalle.NombreProducto,
            Precio = detalle.Precio
        };
        StateHasChanged();
    }

    private void EliminarDetalle(long id)
    {
        var detalle = ListadoDetalles.FirstOrDefault(d => d.IdProducto == id);
        if (detalle != null) ListadoDetalles.Remove(detalle);
        if (PaginaActual > TotalPaginas && TotalPaginas > 0) PaginaActual = TotalPaginas;
        StateHasChanged();
    }

    void PaginaSiguiente()
    {
        if (PaginaActual < TotalPaginas) PaginaActual++;
    }

    void PaginaAnterior()
    {
        if (PaginaActual > 1) PaginaActual--;
    }

}
