using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateProductoDTOValidartor : AbstractValidator<ProductoDTO>
{
    public CreateProductoDTOValidartor()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Descripcion)
            .NotEmpty().WithMessage("{PropertyName} es requerido.");

        RuleFor(p => p.IdCategoria)
            .NotEqual(0).WithMessage("{PropertyName} es requerido.");

        RuleFor(p => p.IdUnidadMedida)
            .NotEqual(0).WithMessage("{PropertyName} es requerido.");

        RuleFor(p => p.Marca)
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

    }
}

