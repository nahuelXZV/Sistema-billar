using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateTransaccionInventarioDTOValidator : AbstractValidator<TransaccionInventarioDTO>
{
    public CreateTransaccionInventarioDTOValidator()
    {
        RuleFor(p => p.Glosa)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Fecha)
            .NotEmpty().WithMessage("{PropertyName} es requerido.");

        RuleFor(p => p.Tipo)
            .NotEqual((short)0)
            .WithMessage("{PropertyName} es requerido.");
    }

}
