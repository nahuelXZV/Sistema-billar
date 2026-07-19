using Domain.DTOs.Sales;
using FluentValidation;
using static Domain.Constants.Constantes;

namespace Domain.Validators.Sales;

public class CreateTurnoCajaDTOValidator : AbstractValidator<TurnoCajaDTO>
{
    public CreateTurnoCajaDTOValidator()
    {
        RuleFor(turno => turno.IdVendedor)
            .GreaterThan(0).WithMessage("Debe seleccionar un vendedor.");

        RuleFor(turno => turno.Observacion)
            .MaximumLength(500).WithMessage("La observación no debe exceder los 500 caracteres.");

        RuleFor(turno => turno.Detalles)
            .NotEmpty().WithMessage("Debe registrar al menos un método de pago.")
            .Must(detalles => detalles is null ||
                detalles.Select(detalle => detalle.IdMetodoPago).Distinct().Count() == detalles.Count)
            .WithMessage("No se puede repetir un método de pago en el turno.");

        RuleForEach(turno => turno.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(item => item.IdMetodoPago)
                .GreaterThan(0).WithMessage("El método de pago es requerido.");

            detalle.RuleFor(item => item.MontoApertura)
                .GreaterThanOrEqualTo(0).WithMessage("El monto de apertura no puede ser negativo.");

            detalle.RuleFor(item => item.MontoVentasSistema)
                .GreaterThanOrEqualTo(0).When(item => item.MontoVentasSistema.HasValue)
                .WithMessage("El monto de ventas no puede ser negativo.");

            detalle.RuleFor(item => item.MontoCierreDeclarado)
                .GreaterThanOrEqualTo(0).When(item => item.MontoCierreDeclarado.HasValue)
                .WithMessage("El monto de cierre no puede ser negativo.");
        });

        When(turno => turno.Estado == (short)EstadoTurnoCaja.Cerrado, () =>
        {
            RuleForEach(turno => turno.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(item => item.MontoCierreDeclarado)
                    .NotNull().WithMessage("Debe registrar el monto de cierre.");
            });
        });
    }
}
