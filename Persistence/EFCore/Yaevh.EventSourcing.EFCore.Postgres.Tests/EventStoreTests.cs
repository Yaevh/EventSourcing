using Microsoft.EntityFrameworkCore;
using Yaevh.EventSourcing.EFCore.Tests;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;

[Collection(nameof(PostgresFixture))]
public class EventStoreTests : EventStoreTestBase
{
    public PostgresFixture Postgres { get; }
    public EventStoreTests(PostgresFixture databaseFixture) : base(databaseFixture)
    {
        Postgres = databaseFixture ?? throw new ArgumentNullException(nameof(databaseFixture));
    }

    protected override async Task<TestDbContext> BuildDbContext(CancellationToken cancellationToken)
    {
        var token = CancellationToken.None;

        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(
                Postgres.ConnectionString,
                options => options.MigrationsAssembly(this.GetType().Assembly.FullName))
            .Options;
        var dbContext = new TestDbContext(dbContextOptions);
        await dbContext.Database.MigrateAsync(cancellationToken);
        return dbContext;
    }
}
