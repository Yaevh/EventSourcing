using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.SQLite.Tests
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
            var connection = new InMemorySqliteConnection();
            var connectionFactory = () => connection;
            var eventSerializer = new SystemTextJsonEventSerializer();

            var aggregate = new BasicAggregate(Guid.NewGuid());
            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            var aggregateStore = new AggregateStore<Guid>(connectionFactory, eventSerializer, new GuidAggregateIdSerializer());
            var aggregateManager = new AggregateManager<BasicAggregate, Guid>(
                aggregateStore,
                new DefaultAggregateFactory(),
                new FakePublisher(),
                new NullLogger<AggregateManager<BasicAggregate, Guid>>());

            await aggregateManager.CommitAsync(aggregate, CancellationToken.None);

            // Act
            var restoredAggregate = await aggregateManager.LoadAsync(aggregate.AggregateId, CancellationToken.None);

            // Assert
            restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
            restoredAggregate.Version.Should().Be(aggregate.Version);
            restoredAggregate.CurrentValue.Should().Be(aggregate.CurrentValue);
            restoredAggregate.UncommittedEvents.Should().BeEmpty();
        }
    }
}