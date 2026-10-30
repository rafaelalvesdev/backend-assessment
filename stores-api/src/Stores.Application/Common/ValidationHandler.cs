using FluentValidation;

namespace Stores.Application.Common;

public sealed class ValidationHandler<TRequest, TResponse>(
    IHandler<TRequest, Result<TResponse>> inner,
    IEnumerable<IValidator<TRequest>> validators) : IHandler<TRequest, Result<TResponse>>
{
    public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return await inner.HandleAsync(request, cancellationToken);

        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return Error.Validation(errors);
    }
}
