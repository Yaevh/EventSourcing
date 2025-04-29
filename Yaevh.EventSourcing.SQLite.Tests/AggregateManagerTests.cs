using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.SQLite.Tests
{
    public class AggregateManagerTests
    {
        private readonly IReadOnlyDictionary<Type, IAggregateIdSerializer> _knownAggregateIdSerializers =
            new Dictionary<Type, IAggregateIdSerializer>() {
                [typeof(Guid)] = new GuidAggregateIdSerializer()
            };


        #region support classes
        private class FakePublisher : IPublisher
        {
            public Task Publish<TAggregateId>(DomainEvent<TAggregateId> @event, CancellationToken cancellationToken)
                => Task.CompletedTask;
        }
        #endregion


        [Fact(DisplayName = "Loaded aggregate should match the stored one")]
        public async Task LoadedAggregateShouldMatchStoredOne()
        {
            // Arrange
            var connection = new InMemorySqliteConnection();
            var connectionFactory = () => connection;
            var eventSerializer = new SystemTextJsonSerializer();

            var aggregate = new BasicAggregate(Guid.NewGuid());
            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            var aggregateStore = new AggregateStore(connectionFactory, eventSerializer, _knownAggregateIdSerializers);
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