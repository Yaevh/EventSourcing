using Microsoft.EntityFrameworkCore;
using Yaevh.EventSourcing.EFCore.Tests;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;

[Collection(nameof(PostgresFixture))]
public class AggregateManagerTests(PostgresFixture databaseFixture) : AggregateManagerTestBase(databaseFixture)
{
    protected override async Task<TestDbContext> BuildDbContext(CancellationToken cancellationToken)
    {
        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(
                DatabaseFixture.ConnectionString,
                options => options.MigrationsAssembly(this.GetType().Assembly.FullName))
            .Options;
        var dbContext = new TestDbContext(dbContextOptions);
        await dbContext.Database.MigrateAsync(cancellationToken);
        return dbContext;
    }

    protected override async Task MigrateDbContext(TestDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}