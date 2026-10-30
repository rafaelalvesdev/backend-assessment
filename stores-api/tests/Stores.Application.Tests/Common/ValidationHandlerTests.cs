using FluentValidation;
using NSubstitute;
using Stores.Application.Common;

namespace Stores.Application.Tests.Common;

public class ValidationHandlerTests
{
    public sealed record Ping(string Name);

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(x => x.Name).NotEmpty();
    }

    private readonly IHandler<Ping, Result<string>> _inner = Substitute.For<IHandler<Ping, Result<string>>>();

    [Fact]
    public async Task Short_circuits_invalid_requests()
    {
        var handler = new ValidationHandler<Ping, string>(_inner, [new PingValidator()]);

        var result = await handler.HandleAsync(new Ping(""), CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("Name"));
        await _inner.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);
    }

    [Fact]
    public async Task Passes_valid_requests_through()
    {
        _inner.HandleAsync(Arg.Any<Ping>(), Arg.Any<CancellationToken>()).Returns(Result<string>.Success("pong"));
        var handler = new ValidationHandler<Ping, string>(_inner, [new PingValidator()]);

        var result = await handler.HandleAsync(new Ping("hi"), CancellationToken.None);

        Assert.Equal("pong", result.Value);
    }
}
