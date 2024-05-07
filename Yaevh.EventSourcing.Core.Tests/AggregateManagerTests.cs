using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace Yaevh.EventSourcing.Core.Tests
{
    public class AggregateManagerTests
    {
        private class FakeAggregateStore : IAggregateStore
        {
            private readonly Dictionary<object, IReadOnlyList<object>> _eventStorage = new();

            public Task<IEnumerable<DomainEvent<TAggregateId>>> LoadAsync<TAggregateId>(TAggregateId aggregateId, CancellationToken cancellationToken)
            {
                var events = _eventStorage[aggregateId] as IReadOnlyList<DomainEvent<TAggregateId>>;
                return Task.FromResult(events.AsEnumerable());
            }

            public Task StoreAsync<TAggregate, TAggregateId>(TAggregate aggregate, IReadOnlyList<DomainEvent<TAggregateId>> events, CancellationToken cancellationToken)
                where TAggregate : IAggregate<TAggregateId>
            {
                var key = aggregate.AggregateId;
                _eventStorage[key] = events;
                return Task.CompletedTask;
            }
        }


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
            var aggregate = new BasicAggregate(Guid.NewGuid());
            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            var aggregateStore = new FakeAggregateStore();
            var aggregateManager = new AggregateManager<BasicAggregate, Guid>(
                aggregateStore,
                new DefaultAggregateFactory(),
                new FakePublisher(),
                new NullLogger<AggregateManager<BasicAggregate, Guid>>());

            await aggregateManager.CommitAsync(aggregate, CancellationToken.None);

            var restoredAggregate = await aggregateManager.LoadAsync(aggregate.AggregateId, CancellationToken.None);


            restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
            restoredAggregate.Version.Should().Be(aggregate.Version);
            restoredAggregate.Value.Should().Be(aggregate.Value);
            restoredAggregate.UncommittedEvents.Should().BeEmpty();
        }
    }
}