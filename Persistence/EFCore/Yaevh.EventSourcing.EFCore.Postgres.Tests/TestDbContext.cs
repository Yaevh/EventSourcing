using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;

//TODO add other DB providers for testing, e.g., SQL Server, SQLite, etc.
public class TestDbContext : EventsDbContext<Guid>
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=postgres")
                .Options;

            return new TestDbContext(options);
        }
    }
}
