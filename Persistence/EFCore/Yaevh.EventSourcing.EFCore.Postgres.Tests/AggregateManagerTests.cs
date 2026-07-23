using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using System.Collections.Generic;
using System.Threading;
using Testcontainers.PostgreSql;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;
{
    public class AggregateManagerTests
    {
        [Fact(DisplayName = "Loaded aggregate should match the stored one")]
        public async Task LoadedAggregateShouldMatchStoredOne()
        {
            // Arrange
            var token = CancellationToken.None;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(60)); // configurable timeout


            // TODO use Testcontainers to start a PostgreSQL container for testing
            await using var postgresContainer = new PostgreSqlBuilder().Build();
            try
            {
                await postgresContainer.StartAsync(cts.Token)
                    .WaitAsync(TimeSpan.FromSeconds(60), cts.Token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to start the PostgreSQL test container. Ensure Docker is running and accessible.", ex);
            }

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            dbContextOptionsBuilder.UseNpgsql(postgresContainer.GetConnectionString());
            var eventSerializer = new SystemTextJsonEventSerializer();
            var dbContext = new TestDbContext(dbContextOptionsBuilder.Options);
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