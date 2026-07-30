using Microsoft.EntityFrameworkCore;
using Yaevh.EventSourcing.EFCore.Tests;

namespace Yaevh.EventSourcing.EFCore.SqlServer.Tests;

[Collection(nameof(MsSqlFixture))]
public class AggregateManagerTests(MsSqlFixture databaseFixture) : AggregateManagerTestBase(databaseFixture)
{
    protected override async Task<TestDbContext> BuildDbContext(CancellationToken cancellationToken)
    {
        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(
                DatabaseFixture.ConnectionString,
                options => options.MigrationsAssembly(this.GetType().Assembly.FullName))
            .Options;
        var dbContext = new TestDbContext(dbContextOptions);
        await dbContext.Database.MigrateAsync(CancellationToken.None);
        return dbContext;
    }

    protected override async Task MigrateDbContext(TestDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}