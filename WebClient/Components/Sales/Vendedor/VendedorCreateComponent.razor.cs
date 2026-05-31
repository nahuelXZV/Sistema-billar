using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.DTOs.Security;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Sales.Vendedor;

public partial class VendedorCreateComponent
{
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IValidator<VendedorDTO> Validator { get; set; }
    [Parameter] public required VendedorDTO Vendedor { get; set; }
    [Parameter] public List<UsuarioDTO> ListaUsuarios { get; set; } = new();
    [Parameter] public List<AlmacenDTO> ListaAlmacenesDisponibles { get; set; } = new();
    [Parameter] public List<ListaPrecioDTO> ListaPreciosDisponibles { get; set; } = new();
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<VendedorCreateComponent>? _objectHelper;
    private FluentValidationValidator<VendedorDTO>? _fvValidator;
    private List<long> ListaAlmacenesSeleccionados { get; set; } = new();
    private bool IsEditing => Vendedor?.Id > 0;

    protected override void OnInitialized()
    {
        Vendedor ??= new VendedorDTO();
        Vendedor.ListaAlmacenes ??= new();
        ListaAlmacenesSeleccionados = Vendedor.ListaAlmacenes
            .Where(p => p.IdAlmacen > 0)
            .Select(p => p.IdAlmacen)
            .Distinct()
            .ToList();

        _editContext = new EditContext(Vendedor);
        _fvValidator = new FluentValidationValidator<VendedorDTO>(_editContext, Validator);
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
            await JSRuntime.InvokeVoidAsync("VendedorCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(VendedorCreateComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private string GetUsuarioText(UsuarioDTO usuario)
    {
        return $"{usuario.Username} - {usuario.Nombre} {usuario.Apellido}";
    }

    private long? GetUsuarioValue(UsuarioDTO usuario)
    {
        return usuario.Id;
    }

    private string GetListaPrecioText(ListaPrecioDTO listaPrecio)
    {
        return listaPrecio.Nombre;
    }

    private long? GetListaPrecioValue(ListaPrecioDTO listaPrecio)
    {
        return listaPrecio.Id;
    }

    private void OnAlmacenSeleccionChanged(long idAlmacen, bool isChecked)
    {
        if (isChecked)
        {
            if (!ListaAlmacenesSeleccionados.Contains(idAlmacen))
                ListaAlmacenesSeleccionados.Add(idAlmacen);
        }
        else
        {
            ListaAlmacenesSeleccionados.Remove(idAlmacen);
        }
    }

    private async Task Guardar()
    {
        try
        {
            Vendedor.ListaAlmacenes = ListaAlmacenesSeleccionados
                .Distinct()
                .Select(idAlmacen => new VendedorAlmacenDTO
                {
                    IdVendedor = Vendedor.Id,
                    IdAlmacen = idAlmacen
                })
                .ToList();
            Vendedor.IdListaPrecio = Vendedor.IdListaPrecio > 0 ? Vendedor.IdListaPrecio : null;

            if (Vendedor.Id != 0)
            {
                Vendedor.UsuarioDTO = null;
                Vendedor.ListaPrecioDTO = null;
                await AppServices.VendedorService.Update(Vendedor);
            }
            else
            {
                Vendedor.UsuarioDTO = null;
                Vendedor.ListaPrecioDTO = null;
                await AppServices.VendedorService.Create(Vendedor);
            }

            await ShowSuccessMessage("Vendedor guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Vendedor/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }
}
