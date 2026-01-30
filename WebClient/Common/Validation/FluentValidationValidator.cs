using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace WebClient.Common.Validation;

public class FluentValidationValidator<T> : IDisposable
{
    private readonly IValidator<T> _validator;
    private readonly EditContext _editContext;
    private ValidationMessageStore _messages;

    public FluentValidationValidator(EditContext editContext, IValidator<T> validator)
    {
        _editContext = editContext;
        _validator = validator;
        _messages = new ValidationMessageStore(editContext);

        _editContext.OnValidationRequested += ValidateModel;
        _editContext.OnFieldChanged += ValidateField;
    }

    private void ValidateModel(object? sender, ValidationRequestedEventArgs e)
    {
        _messages.Clear();

        var result = _validator.Validate((T)_editContext.Model);

        foreach (var error in result.Errors)
        {
            var field = new FieldIdentifier(_editContext.Model, error.PropertyName);
            _messages.Add(field, error.ErrorMessage);
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void ValidateField(object? sender, FieldChangedEventArgs e)
    {
        _messages.Clear(e.FieldIdentifier);

        var result = _validator.Validate((T)_editContext.Model);

        foreach (var error in result.Errors.Where(x => x.PropertyName == e.FieldIdentifier.FieldName))
        {
            _messages.Add(e.FieldIdentifier, error.ErrorMessage);
        }

        _editContext.NotifyValidationStateChanged();
    }

    public void Dispose()
    {
        _editContext.OnValidationRequested -= ValidateModel;
        _editContext.OnFieldChanged -= ValidateField;
    }
}