using Domain.DTOs.Sales;
using FluentValidation;

namespace Domain.Validators.Sales;

public class CreateVendedorDTOValidator : AbstractValidator<VendedorDTO>
{
    public CreateVendedorDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Documento)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.IdUsuario)
            .GreaterThan(0)
            .When(p => p.IdUsuario.HasValue)
            .WithMessage("{PropertyName} no es valido.");

        RuleForEach(p => p.ListaAlmacenes)
            .Must(p => p.IdAlmacen > 0)
            .WithMessage("El almacen seleccionado no es valido.");

        RuleFor(p => p.IdListaPrecio)
            .GreaterThan(0)
            .When(p => p.IdListaPrecio.HasValue)
            .WithMessage("{PropertyName} no es valido.");
    }
}
