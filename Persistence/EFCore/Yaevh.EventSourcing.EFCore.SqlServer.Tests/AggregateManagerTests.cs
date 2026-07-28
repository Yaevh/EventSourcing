using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.SqlServer.Tests;

[Collection("MsSql container collection")]
public class AggregateManagerTests : IAsyncLifetime
{
    public MsSqlFixture MsSql { get; }
    public AggregateManagerTests(MsSqlFixture fixture)
    {
        MsSql = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public async Task InitializeAsync()
    {
        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(MsSql.ConnectionString).Options;
        var dbContext = new TestDbContext(dbContextOptions);
        await dbContext.Database.MigrateAsync(CancellationToken.None);
        dbContext.Events.RemoveRange(dbContext.Events);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public Task DisposeAsync() => Task.CompletedTask;


    [Fact(DisplayName = "Loaded aggregate should match the stored one")]
    public async Task LoadedAggregateShouldMatchStoredOne()
    {
        // Arrange
        var token = CancellationToken.None;
        
        var eventSerializer = new SystemTextJsonEventSerializer();
        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(MsSql.ConnectionString).Options;
        var dbContext = new TestDbContext(dbContextOptions);
        var eventStore = new DbContextEventStore<TestDbContext, Guid>(dbContext, eventSerializer);

        var aggregateId = Guid.NewGuid();
        var aggregate = new CalculationAggregate(aggregateId);
        aggregate.Add(5);
        aggregate.Subtract(2);
        aggregate.Multiply(4);
        aggregate.Divide(3);

        var aggregateManager = new AggregateManager<CalculationAggregate, Guid>(
            eventStore,
            new DefaultAggregateFactory(),
            new NullPublisher(),
            new NullLogger<AggregateManager<CalculationAggregate, Guid>>());

        await aggregateManager.CommitAsync(aggregate, token);

        await dbContext.SaveChangesAsync();

        // Act
        var restoredAggregate = await aggregateManager.LoadAsync(aggregate.AggregateId, token);

        // Assert
        restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
        restoredAggregate.Version.Should().Be(aggregate.Version);
        restoredAggregate.Value.Should().Be(aggregate.Value);
        restoredAggregate.UncommittedEvents.Should().BeEmpty();
    }
}