using Testcontainers.PostgreSql;
using Yaevh.EventSourcing.EFCore.Tests;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;

public class PostgresFixture : IDatabaseFixture, IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true) // optional, cleans volumes
            .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync() => await Container.StartAsync();

    public async Task DisposeAsync() => await Container.DisposeAsync();
}

[CollectionDefinition(nameof(PostgresFixture))]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}