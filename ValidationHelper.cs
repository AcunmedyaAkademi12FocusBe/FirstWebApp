using System.ComponentModel.DataAnnotations;

namespace FirstWebApp;

public static class ValidationHelper
{
    public static bool ValidateModel<T> (T model, out List<ValidationResult> validationResults)
    {
        var validationContext = new ValidationContext(model);
        validationResults = [];
        return Validator.TryValidateObject(model, validationContext, validationResults, true);
    }
}