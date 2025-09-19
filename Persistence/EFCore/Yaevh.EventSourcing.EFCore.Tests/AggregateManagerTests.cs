using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using Testcontainers.PostgreSql;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Tests
{
    public class AggregateManagerTests
    {
        [Fact(DisplayName = "Loaded aggregate should match the stored one")]
        public async Task LoadedAggregateShouldMatchStoredOne()
        {
            // Arrange
            var token = CancellationToken.None;

            await using var postgresContainer = new PostgreSqlBuilder().Build();
            await postgresContainer.StartAsync(token);
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            dbContextOptionsBuilder.UseNpgsql(postgresContainer.GetConnectionString());
            var eventSerializer = new SystemTextJsonEventSerializer();
            var dbContext = new TestDbContext(dbContextOptionsBuilder.Options, eventSerializer);
            await dbContext.Database.MigrateAsync(token);
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
}