using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Yaevh.EventSourcing.Persistence;
using Yaevh.EventSourcing.SQLite;

namespace Yaevh.EventSourcing.SQLite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteEventStore(this IServiceCollection services, Func<IDbConnection> dbConnectionFactory)
    {
        services.AddSQLiteEventStoreBase()
            .AddSingleton(dbConnectionFactory);
        return services;
    }

    public static IServiceCollection AddSQLiteEventStore(this IServiceCollection services, string connectionString)
    {
        services.AddSQLiteEventStoreBase()
            .AddScoped(_ => new SqliteConnection(connectionString))
            .AddScoped(sp => new Func<IDbConnection>(() => sp.GetRequiredService<SqliteConnection>()));
        return services;
    }

    private static IServiceCollection AddSQLiteEventStoreBase(this IServiceCollection services)
    {
        services.AddScoped(typeof(IEventStore<>), typeof(EventStore<>));
        services.AddSingleton<IEventSerializer, SystemTextJsonEventSerializer>();
        services.AddSingleton<IAggregateIdSerializer<Guid>, GuidAggregateIdSerializer>();
        return services;
    }
}
