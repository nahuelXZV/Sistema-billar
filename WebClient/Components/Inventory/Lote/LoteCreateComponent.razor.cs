using System.ComponentModel.DataAnnotations;
using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebClient.Common.Validation;

namespace WebClient.Components.Inventory.Lote;

public partial class LoteCreateComponent
{
    [Inject] public IValidator<LoteDTO> Validator { get; set; }
    [Parameter] public LoteDTO Lote { get; set; } = new LoteDTO();
    [Parameter] public EventCallback<LoteDTO> OnLoteCreated { get; set; }
    [Parameter] public long IdProducto { get; set; }
    private EditContext? _editContext { get; set; }
    private FluentValidationValidator<LoteDTO> _fvValidator;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Lote);
        _fvValidator = new FluentValidationValidator<LoteDTO>(_editContext, Validator);
    }

    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            Lote.IdProducto = IdProducto;
            if (Lote.Id != 0)
            {
                var respuesta = await AppServices.LoteService.Update(Lote);
            }
            else
            {
                var respuesta = await AppServices.LoteService.Create(Lote);
                Lote.Id = respuesta; // Asignar el ID generado al DTO
            }
            await ShowSuccessMessage("Lote guardado correctamente");
            await OnLoteCreated.InvokeAsync(Lote);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }


}
