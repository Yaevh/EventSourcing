using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Tests;

public abstract class AggregateManagerTestBase : IAsyncLifetime
{
    public IDatabaseFixture DatabaseFixture { get; }
    public AggregateManagerTestBase(IDatabaseFixture fixture)
    {
        DatabaseFixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    protected abstract Task<TestDbContext> BuildDbContext(CancellationToken cancellationToken);
    protected abstract Task MigrateDbContext(TestDbContext dbContext, CancellationToken cancellationToken);

    public async Task InitializeAsync()
    {
        var cancellationToken = CancellationToken.None;

        var dbContext = await BuildDbContext(cancellationToken);
        await MigrateDbContext(dbContext, cancellationToken);
        dbContext.Events.RemoveRange(dbContext.Events);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DisposeAsync() => Task.CompletedTask;


    [Fact(DisplayName = "Loaded aggregate should match the stored one")]
    public async Task LoadedAggregateShouldMatchStoredOne()
    {
        // Arrange
        var token = CancellationToken.None;
        
        var dbContext = await BuildDbContext(token);
        var eventSerializer = new SystemTextJsonEventSerializer();

        var eventStore = new DbContextEventStore<TestDbContext, Guid>(
            dbContext, eventSerializer,
            new DefaultAggregateTypeNamingStrategy(),
            new DefaultEventTypeNamingStrategy());

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