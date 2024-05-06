using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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

        private class FakePublisher : IPublisher
        {
            public Task Publish<TAggregateId>(DomainEvent<TAggregateId> @event, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private class NullLogger<TCategory> : Microsoft.Extensions.Logging.ILogger<TCategory>
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => throw new NotImplementedException();
        }


        [Fact(DisplayName = "Loaded aggregate should match the stored one")]
        public async Task LoadedAggregateShouldMatchStoredOne()
        {
            var connection = new NonClosingSqliteConnection("DataSource=:memory:");
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

            var restoredAggregate = await aggregateManager.LoadAsync(aggregate.AggregateId, CancellationToken.None);


            restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
            restoredAggregate.Version.Should().Be(aggregate.Version);
            restoredAggregate.CurrentValue.Should().Be(aggregate.CurrentValue);
            restoredAggregate.UncommittedEvents.Should().BeEmpty();
        }


        private class NonClosingSqliteConnection : Microsoft.Data.Sqlite.SqliteConnection
        {
            public NonClosingSqliteConnection(string connectionString) : base(connectionString) { }
            public override void Close() { }
        }
    }
}