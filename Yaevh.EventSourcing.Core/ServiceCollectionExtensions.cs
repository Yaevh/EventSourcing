using Microsoft.Extensions.DependencyInjection;

namespace Yaevh.EventSourcing.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultAggregateManager(this IServiceCollection services)
    {
        services.AddScoped(typeof(IAggregateManager<,>), typeof(AggregateManager<,>));
        services.AddScoped<IAggregateFactory, DefaultAggregateFactory>();

        return services;
    }

    public static IServiceCollection AddNullPublisher(this IServiceCollection services)
    {
        return services.AddScoped<IPublisher, NullPublisher>();
    }
}
