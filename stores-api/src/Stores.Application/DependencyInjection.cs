using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stores.Application.Common;

namespace Stores.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton);
        services.AddSingleton(TimeProvider.System);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var type in handlerTypes)
        {
            foreach (var contract in type.GetInterfaces().Where(IsHandlerContract))
            {
                services.AddScoped(type);
                services.AddScoped(contract, sp => Decorate(sp, contract, sp.GetRequiredService(type)));
            }
        }

        return services;
    }

    private static bool IsHandlerContract(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IHandler<,>);

    private static object Decorate(IServiceProvider services, Type contract, object handler)
    {
        var (request, response) = (contract.GenericTypeArguments[0], contract.GenericTypeArguments[1]);

        if (!response.IsGenericType || response.GetGenericTypeDefinition() != typeof(Result<>))
            return handler;

        var decorator = typeof(ValidationHandler<,>).MakeGenericType(request, response.GenericTypeArguments[0]);
        return ActivatorUtilities.CreateInstance(services, decorator, handler);
    }
}
