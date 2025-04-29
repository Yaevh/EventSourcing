using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Tests
{
    public class AggregateManagerTests
    {
        #region supporting classes
        private class FakePublisher : IPublisher
        {
            public Task Publish<TAggregateId>(AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
                where TAggregateId : notnull
                => Task.CompletedTask;
        }
        #endregion


        [Fact(DisplayName = "Loaded aggregate should match the stored one")]
        public async Task LoadedAggregateShouldMatchStoredOne()
        {
            // Arrange
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<EventsDbContext<Guid>>();
            dbContextOptionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var eventSerializer = new SystemTextJsonEventSerializer();
            var dbContext = new EventsDbContext<Guid>(dbContextOptionsBuilder.Options, eventSerializer);
            var aggregateStore = new DbContextAggregateStore<EventsDbContext<Guid>, Guid>(dbContext, eventSerializer);

            var aggregateId = Guid.NewGuid();
            var aggregate = new CalculationAggregate(aggregateId);
            aggregate.Add(5);
            aggregate.Subtract(2);
            aggregate.Multiply(4);
            aggregate.Divide(3);

            var aggregateManager = new AggregateManager<CalculationAggregate, Guid>(
                aggregateStore,
                new DefaultAggregateFactory(),
                new FakePublisher(),
                new NullLogger<AggregateManager<CalculationAggregate, Guid>>());

            await aggregateManager.CommitAsync(aggregate, CancellationToken.None);

            await dbContext.SaveChangesAsync();

            // Act
            var restoredAggregate = await aggregateManager.LoadAsync(aggregate.AggregateId, CancellationToken.None);

            // Assert
            restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
            restoredAggregate.Version.Should().Be(aggregate.Version);
            restoredAggregate.Value.Should().Be(aggregate.Value);
            restoredAggregate.UncommittedEvents.Should().BeEmpty();
        }
    }
}