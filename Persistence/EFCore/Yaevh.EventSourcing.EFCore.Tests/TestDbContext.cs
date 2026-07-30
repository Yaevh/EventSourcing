using Microsoft.EntityFrameworkCore;

namespace Yaevh.EventSourcing.EFCore.Tests;

public class TestDbContext : EventsDbContext<Guid>
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
}
