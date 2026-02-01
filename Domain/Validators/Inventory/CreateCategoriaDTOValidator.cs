using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateCategoriaDTOValidator : AbstractValidator<CategoriaDTO>
{
    public CreateCategoriaDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Descripcion)
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");
    }
}
