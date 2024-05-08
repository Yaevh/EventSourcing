using FluentAssertions;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Yaevh.EventSourcing.Core.Tests
{
    public class AggregateManagerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public AggregateManagerTests(ITestOutputHelper testOutputHelper)
            => _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

        #region support classes
        private class FakeAggregateStore : IAggregateStore
        {
            private readonly Dictionary<object, IReadOnlyList<object>> _eventStorage = new();

            public Task<IEnumerable<DomainEvent<TAggregateId>>> LoadAsync<TAggregateId>(TAggregateId aggregateId, CancellationToken cancellationToken)
                where TAggregateId : notnull
            {
                var events = (IReadOnlyList<DomainEvent<TAggregateId>>)_eventStorage[aggregateId];
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
            var aggregate = new BasicAggregate(Guid.NewGuid());
            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            var aggregateStore = new FakeAggregateStore();

            using ILoggerFactory factory = LoggerFactory.Create(builder
                => builder.AddXUnit(_testOutputHelper).SetMinimumLevel(LogLevel.Trace));
            var logger = factory.CreateLogger<AggregateManager<BasicAggregate, Guid>>();

            var aggregateManager = new AggregateManager<BasicAggregate, Guid>(
                aggregateStore,
                new DefaultAggregateFactory(),
                new FakePublisher(),
                logger);

            await aggregateManager.CommitAsync(aggregate, CancellationToken.None);

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