using FluentValidation.Results;

namespace Products.Functions.Common;

internal static class ValidationExtensions
{
    public static Error ToError(this ValidationResult result) =>
        Error.Validation(result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()));
}
