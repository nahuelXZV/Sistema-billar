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
    private long IdProductoSeleccionado { get; set; }
    private long? IdProductoConversionEnEdicion { get; set; }
    private ListaPrecioDTO? _listaPrecioInicializada;
    private int PaginaActual = 1;
    private int RegistrosPorPagina = 5;
    private List<ProductoConversionDTO> ConversionesProductoSeleccionado => ListadoProductos
            .FirstOrDefault(producto => producto.Id == IdProductoSeleccionado)?
            .ProductoConversiones?
            .Where(conversion => conversion.Id > 0)
            .OrderBy(conversion => conversion.FactorConversion)
            .ToList() ?? [];
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

        if (ReferenceEquals(_listaPrecioInicializada, ListaPrecio))
            return;

        _listaPrecioInicializada = ListaPrecio;
        ListadoDetalles = [];

        foreach (var item in ListaPrecio.ListaDetalles ?? [])
        {
            var conversion = BuscarConversion(item.IdProductoConversion) ?? item.ProductoConversion;
            var producto = ListadoProductos.FirstOrDefault(
                producto => producto.Id == conversion?.IdProducto);

            ListadoDetalles.Add(new ListaPrecioDetalleDTO
            {
                Id = item.Id,
                IdListaPrecio = item.IdListaPrecio,
                IdProductoConversion = item.IdProductoConversion,
                Precio = item.Precio,
                NombreProducto = producto?.Nombre ?? item.NombreProducto,
                NombreUnidadMedida = conversion?.UnidadMedida?.Nombre ?? item.NombreUnidadMedida,
                AbreviaturaUnidadMedida = conversion?.UnidadMedida?.Abreviatura ?? item.AbreviaturaUnidadMedida,
                FactorConversion = conversion?.FactorConversion ?? item.FactorConversion,
                ProductoConversion = conversion
            });
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
        if (IdProductoSeleccionado == 0 || DetalleDTO.IdProductoConversion == 0 || DetalleDTO.Precio <= 0)
        {
            await ShowErrorMessage("Debe seleccionar un producto, una unidad de medida y un precio mayor a cero.");
            return;
        }

        var producto = ListadoProductos.FirstOrDefault(item => item.Id == IdProductoSeleccionado);

        if (producto == null)
        {
            await ShowErrorMessage("Producto no encontrado.");
            return;
        }

        var conversion = producto.ProductoConversiones?.FirstOrDefault(item => item.Id == DetalleDTO.IdProductoConversion);

        if (conversion == null)
        {
            await ShowErrorMessage("La unidad seleccionada no pertenece al producto.");
            return;
        }

        var detalleExistente = ListadoDetalles.FirstOrDefault(item => item.IdProductoConversion == conversion.Id);

        var detalleEnEdicion = IdProductoConversionEnEdicion.HasValue
            ? ListadoDetalles.FirstOrDefault(item => item.IdProductoConversion == IdProductoConversionEnEdicion.Value) : null;

        if (detalleEnEdicion != null)
        {
            if (detalleExistente != null && !ReferenceEquals(detalleExistente, detalleEnEdicion))
            {
                await ShowErrorMessage("El producto ya tiene un precio para la unidad seleccionada.");
                return;
            }

            detalleEnEdicion.IdProductoConversion = conversion.Id;
            detalleEnEdicion.Precio = DetalleDTO.Precio;
            detalleEnEdicion.NombreProducto = producto.Nombre;
            detalleEnEdicion.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? string.Empty;
            detalleEnEdicion.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? string.Empty;
            detalleEnEdicion.FactorConversion = conversion.FactorConversion;
            detalleEnEdicion.ProductoConversion = conversion;
        }
        else if (detalleExistente != null)
        {
            detalleExistente.Precio = DetalleDTO.Precio;
        }
        else
        {
            ListadoDetalles.Add(new ListaPrecioDetalleDTO
            {
                IdProductoConversion = conversion.Id,
                Precio = DetalleDTO.Precio,
                NombreProducto = producto.Nombre,
                NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? string.Empty,
                AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? string.Empty,
                FactorConversion = conversion.FactorConversion,
                ProductoConversion = conversion
            });
        }

        IdProductoSeleccionado = 0;
        IdProductoConversionEnEdicion = null;
        DetalleDTO = new ListaPrecioDetalleDTO();
        StateHasChanged();
    }

    private async Task EditarDetalle(long idProductoConversion)
    {
        var detalle = ListadoDetalles.FirstOrDefault(
            item => item.IdProductoConversion == idProductoConversion);

        if (detalle == null)
        {
            await ShowErrorMessage("Detalle no encontrado.");
            return;
        }

        var conversion = BuscarConversion(idProductoConversion) ?? detalle.ProductoConversion;
        if (conversion == null)
        {
            await ShowErrorMessage("La unidad configurada ya no está disponible.");
            return;
        }

        IdProductoSeleccionado = conversion.IdProducto;
        IdProductoConversionEnEdicion = detalle.IdProductoConversion;
        DetalleDTO = new ListaPrecioDetalleDTO
        {
            Id = detalle.Id,
            IdListaPrecio = detalle.IdListaPrecio,
            IdProductoConversion = detalle.IdProductoConversion,
            NombreProducto = detalle.NombreProducto,
            NombreUnidadMedida = detalle.NombreUnidadMedida,
            AbreviaturaUnidadMedida = detalle.AbreviaturaUnidadMedida,
            FactorConversion = detalle.FactorConversion,
            Precio = detalle.Precio,
            ProductoConversion = conversion
        };

        StateHasChanged();
    }

    private void EliminarDetalle(long idProductoConversion)
    {
        var detalle = ListadoDetalles.FirstOrDefault(
            item => item.IdProductoConversion == idProductoConversion);

        if (detalle != null) ListadoDetalles.Remove(detalle);

        if (IdProductoConversionEnEdicion == idProductoConversion)
        {
            IdProductoSeleccionado = 0;
            IdProductoConversionEnEdicion = null;
            DetalleDTO = new ListaPrecioDetalleDTO();
        }

        if (PaginaActual > TotalPaginas && TotalPaginas > 0) PaginaActual = TotalPaginas;
        StateHasChanged();
    }

    private void ProductoSeleccionado()
    {
        DetalleDTO.IdProductoConversion = 0;
    }

    private ProductoConversionDTO? BuscarConversion(long idProductoConversion) =>
        ListadoProductos
            .SelectMany(producto => producto.ProductoConversiones ?? [])
            .FirstOrDefault(conversion => conversion.Id == idProductoConversion);

    private static string ObtenerEtiquetaConversion(ProductoConversionDTO conversion)
    {
        var nombreUnidad = conversion.UnidadMedida?.Nombre ?? "Unidad";
        return conversion.FactorConversion == 1
            ? $"{nombreUnidad} (base)"
            : $"{nombreUnidad} (x{conversion.FactorConversion})";
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
