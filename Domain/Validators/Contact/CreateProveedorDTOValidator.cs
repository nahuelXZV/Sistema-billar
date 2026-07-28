using Domain.DTOs.Contact;
using FluentValidation;

namespace Domain.Validators.Contact;

public class CreateProveedorDTOValidator : AbstractValidator<ProveedorDTO>
{
    public CreateProveedorDTOValidator()
    {
        RuleFor(proveedor => proveedor.NombreComercial)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(proveedor => proveedor.NombreContacto)
            .MaximumLength(200).WithMessage("{PropertyName} no debe exceder los 200 caracteres.");

        RuleFor(proveedor => proveedor.Telefono)
            .MaximumLength(50).WithMessage("{PropertyName} no debe exceder los 50 caracteres.");

        RuleFor(proveedor => proveedor.Direccion)
            .MaximumLength(500).WithMessage("{PropertyName} no debe exceder los 500 caracteres.");

        RuleForEach(proveedor => proveedor.ListaProductos)
            .ChildRules(costo =>
            {
                costo.RuleFor(item => item.IdProducto)
                    .GreaterThan(0).WithMessage("El producto seleccionado no es válido.");

                costo.RuleFor(item => item.IdProductoConversion)
                    .GreaterThan(0).When(item => item.IdProductoConversion.HasValue)
                    .WithMessage("La conversión seleccionada no es válida.");

                costo.RuleFor(item => item.CostoReferencial)
                    .GreaterThan(0).WithMessage("El costo referencial debe ser mayor a cero.");
            });
    }
}
