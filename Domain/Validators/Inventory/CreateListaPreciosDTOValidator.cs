
using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateListaPreciosDTOValidator : AbstractValidator<ListaPrecioDTO>
{
    public CreateListaPreciosDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Descripcion)
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");
    }
}
