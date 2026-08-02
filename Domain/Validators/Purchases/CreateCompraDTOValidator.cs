using Domain.DTOs.Purchases;
using FluentValidation;

namespace Domain.Validators.Purchases;

public class CreateCompraDTOValidator : AbstractValidator<CompraDTO>
{
    public CreateCompraDTOValidator()
    {
        RuleFor(compra => compra.IdempotencyKey)
            .NotEmpty().WithMessage("La clave de idempotencia es requerida.");

        RuleFor(compra => compra.IdProveedor)
            .GreaterThan(0).WithMessage("El proveedor seleccionado no es válido.");

        RuleFor(compra => compra.IdAlmacen)
            .GreaterThan(0).WithMessage("El almacén seleccionado no es válido.");

        RuleFor(compra => compra.IdUsuario)
            .GreaterThan(0).WithMessage("El usuario no es válido.");

        RuleFor(compra => compra.Observacion)
            .MaximumLength(1000).WithMessage("La observación no debe exceder los 1000 caracteres.");

        RuleFor(compra => compra.ListaDetalles)
            .NotEmpty().WithMessage("La compra debe tener al menos un detalle.");

        RuleForEach(compra => compra.ListaDetalles)
            .ChildRules(detalle =>
            {
                detalle.RuleFor(item => item.IdProducto)
                    .GreaterThan(0).WithMessage("El producto seleccionado no es válido.");

                detalle.RuleFor(item => item.IdProductoConversion)
                    .GreaterThan(0).When(item => item.IdProductoConversion.HasValue)
                    .WithMessage("La conversión seleccionada no es válida.");

                detalle.RuleFor(item => item.IdLote)
                    .GreaterThan(0).When(item => item.IdLote.HasValue)
                    .WithMessage("El lote seleccionado no es válido.");

                detalle.RuleFor(item => item.Cantidad)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

                detalle.RuleFor(item => item.CostoUnitario)
                    .GreaterThan(0).WithMessage("El costo unitario debe ser mayor a cero.");

                detalle.RuleFor(item => item.Descuento)
                    .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");
            });
    }
}
