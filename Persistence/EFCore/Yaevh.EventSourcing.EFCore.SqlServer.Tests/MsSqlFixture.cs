using Testcontainers.MsSql;

namespace Yaevh.EventSourcing.EFCore.SqlServer.Tests;

public class MsSqlFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; } = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Your_password123")
            .WithEnvironment("MSSQL_PID", "Express") // optional but recommended
            .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync() => await Container.StartAsync();

    public async Task DisposeAsync() => await Container.DisposeAsync();
}

[CollectionDefinition("MsSql container collection")]
public class MsSqlCollection : ICollectionFixture<MsSqlFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}