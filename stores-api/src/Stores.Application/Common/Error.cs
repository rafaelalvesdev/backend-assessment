namespace Stores.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record Error(
    ErrorType Type,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);

    public static Error Conflict(string code, string message) => new(ErrorType.Conflict, code, message);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ErrorType.Validation, "validation_failed", "One or more validation errors occurred.", errors);
}
