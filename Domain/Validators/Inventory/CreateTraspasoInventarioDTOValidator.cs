using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateTraspasoInventarioDTOValidator : AbstractValidator<TraspasoInventarioDTO>
{
    public CreateTraspasoInventarioDTOValidator()
    {
        RuleFor(traspaso => traspaso.IdAlmacenOrigen)
            .GreaterThan(0).WithMessage("{PropertyName} es requerido.");

        RuleFor(traspaso => traspaso.IdAlmacenDestino)
            .GreaterThan(0).WithMessage("{PropertyName} es requerido.")
            .NotEqual(traspaso => traspaso.IdAlmacenOrigen)
            .WithMessage("El almacén origen y el almacén destino deben ser diferentes.");

        RuleFor(traspaso => traspaso.Fecha)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} es requerido.");

        RuleFor(traspaso => traspaso.Glosa)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(traspaso => traspaso.Detalles)
            .NotEmpty().WithMessage("Debe agregar al menos un detalle al traspaso.");

        RuleForEach(traspaso => traspaso.Detalles)
            .ChildRules(detalle =>
            {
                detalle.RuleFor(item => item.IdProducto)
                    .GreaterThan(0).WithMessage("{PropertyName} es requerido.");

                detalle.RuleFor(item => item.Cantidad)
                    .GreaterThan(0).WithMessage("{PropertyName} debe ser mayor a cero.");
            });
    }
}
