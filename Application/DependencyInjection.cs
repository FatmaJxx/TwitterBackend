using System.Reflection;
using Application.Abstraction.Mediator;
using Application.Auth.Commands.Register;
using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(RegisterHandler).Assembly);
        return services;
    }

    private static IServiceCollection AddMediator(
            this IServiceCollection services,
            params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsInterface: false })
            .Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition()
            == typeof(IRequestHandler<,>))).ToList();


        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddTransient(interfaceType, handlerType);
        }
        
        services.AddValidatorsFromAssemblies(assemblies);

        services.AddTransient(
            typeof(IPipelineBehavior<,>), 
            typeof(LoggingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ExceptionHandlingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(TransactionBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(IdempotencyBehavior<,>));

        return services;
    }
}

