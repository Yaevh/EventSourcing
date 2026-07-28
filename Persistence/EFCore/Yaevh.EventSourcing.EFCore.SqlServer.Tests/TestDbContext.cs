using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Yaevh.EventSourcing.EFCore.SqlServer.Tests;

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
                .UseSqlServer("Host=localhost;Database=test;Username=MsSql;Password=MsSql")
                .Options;

            return new TestDbContext(options);
        }
    }
}
