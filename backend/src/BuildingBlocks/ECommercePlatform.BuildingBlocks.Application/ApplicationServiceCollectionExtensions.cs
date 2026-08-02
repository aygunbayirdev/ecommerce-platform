using ECommercePlatform.BuildingBlocks.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.BuildingBlocks.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
