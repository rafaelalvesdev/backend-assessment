namespace Products.Functions.Common;

public class Result
{
    private readonly Error? _error;

    protected Result(Error? error) => _error = error;

    public bool IsSuccess => _error is null;

    public Error Error => _error ?? throw new InvalidOperationException("A successful result has no error.");

    public static Result Success() => new(null);

    public static implicit operator Result(Error error) => new(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, Error? error) : base(error) => _value = value;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(Error error) => new(default, error);
}
