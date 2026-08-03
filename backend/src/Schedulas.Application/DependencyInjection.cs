using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.RuleEngine;
using Schedulas.Domain.RuleEngine;

namespace Schedulas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(config => config.AddMaps(assembly));

        // Pipeline order matters: logging wraps everything, then tenant
        // authorization, then validation, then the handler itself
        // (Architecture §3.2, §6).
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantAuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));

        services.AddScoped<IRuleEngine, RuleEngineOrchestrator>();

        return services;
    }
}
