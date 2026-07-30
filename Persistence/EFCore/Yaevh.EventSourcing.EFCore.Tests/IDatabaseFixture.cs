namespace Yaevh.EventSourcing.EFCore.Tests;

public interface IDatabaseFixture : IAsyncLifetime
{
    string ConnectionString { get; }
}