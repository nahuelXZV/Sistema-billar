using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebClient.Common.Validation;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class BuscarClienteComponent : IDisposable
{
    [Inject] public IValidator<ClienteDTO> Validator { get; set; } = default!;
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<ClienteDTO?> OnClienteSelected { get; set; }

    private const int SearchLimit = 6;
    private List<ClienteDTO> Clientes { get; set; } = [];
    private ClienteDTO NuevoCliente { get; set; } = new();
    private EditContext? _createEditContext;
    private FluentValidationValidator<ClienteDTO>? _fluentValidator;
    private string SearchText { get; set; } = string.Empty;
    private bool IsResultsOpen { get; set; }
    private bool IsCreateModalOpen { get; set; }
    private bool IsSearching { get; set; }
    private bool IsSaving { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ResetCreateForm();
        SyncSelectedClientText();
    }

    protected override void OnParametersSet()
    {
        SyncSelectedClientText();
    }

    private async Task OpenResultsAsync()
    {
        IsResultsOpen = true;

        if (Clientes.Count == 0)
        {
            await SearchClientesAsync(SearchText);
        }
    }

    private async Task SearchClientesAsync(ChangeEventArgs args)
    {
        SearchText = args.Value?.ToString() ?? string.Empty;
        PuntoVenta.ClienteSeleccionado = null;
        await OnClienteSelected.InvokeAsync(null);
        await SearchClientesAsync(SearchText);
    }

    private async Task SearchClientesAsync(string search)
    {
        IsResultsOpen = true;
        IsSearching = true;

        try
        {
            var response = await AppServices.ClienteService.GetAll(new FilterDTO
            {
                Limit = SearchLimit,
                Search = search
            });

            Clientes = response.Data;
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task SelectClienteAsync(ClienteDTO cliente)
    {
        PuntoVenta.ClienteSeleccionado = cliente;
        SearchText = GetClienteDisplay(cliente);
        Clientes.Clear();
        IsResultsOpen = false;
        await OnClienteSelected.InvokeAsync(cliente);
    }

    private async Task ClearSelectedClientAsync()
    {
        PuntoVenta.ClienteSeleccionado = null;
        SearchText = string.Empty;
        Clientes.Clear();
        IsResultsOpen = false;
        await OnClienteSelected.InvokeAsync(null);
    }

    private async Task CloseResultsOnFocusOutAsync()
    {
        await Task.Delay(120);
        IsResultsOpen = false;
        await InvokeAsync(StateHasChanged);
    }

    private void OpenCreateModal()
    {
        ResetCreateForm();
        IsCreateModalOpen = true;
        IsResultsOpen = false;
    }

    private void CloseCreateModal()
    {
        IsCreateModalOpen = false;
    }

    private async Task CreateClienteAsync()
    {
        if (IsSaving || _createEditContext is null || !_createEditContext.Validate())
        {
            return;
        }

        IsSaving = true;

        try
        {
            var cliente = new ClienteDTO
            {
                Nombre = NuevoCliente.Nombre.Trim(),
                Documento = NuevoCliente.Documento.Trim(),
                Telefono = NuevoCliente.Telefono.Trim()
            };

            cliente.Id = await AppServices.ClienteService.Create(cliente);
            await SelectClienteAsync(cliente);
            IsCreateModalOpen = false;
            await ShowSuccessMessage("Cliente creado correctamente");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ResetCreateForm()
    {
        _fluentValidator?.Dispose();
        NuevoCliente = new ClienteDTO();
        _createEditContext = new EditContext(NuevoCliente);
        _fluentValidator = new FluentValidationValidator<ClienteDTO>(_createEditContext, Validator);
    }

    private void SyncSelectedClientText()
    {
        if (PuntoVenta.ClienteSeleccionado is not null)
        {
            SearchText = GetClienteDisplay(PuntoVenta.ClienteSeleccionado);
        }
    }

    private static string GetClienteDisplay(ClienteDTO cliente)
    {
        return string.IsNullOrWhiteSpace(cliente.Documento)
            ? cliente.Nombre
            : $"{cliente.Nombre} - {cliente.Documento}";
    }

    private static string GetClienteDetail(ClienteDTO cliente)
    {
        var values = new[] { cliente.Documento, cliente.Telefono }
            .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(" | ", values);
    }

    public void Dispose()
    {
        _fluentValidator?.Dispose();
    }
}
