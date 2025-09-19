using Dapper;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.SQLite.Tests
{
    public class EventStoreTests
    {
        [Fact(DisplayName = "A01. Database is sane: can be created and queried")]
        public async Task DatabaseCanBeCreatedAndQueried()
        {
            // Arrange
            var connection = new InMemorySqliteConnection();
            var connectionFactory = () => connection;
            var eventSerializer = new SystemTextJsonEventSerializer();

            var eventStore = new EventStore<Guid>(connectionFactory, eventSerializer, new GuidAggregateIdSerializer());

            // Act & Assert - should not throw
            var events = await eventStore.LoadAsync(Guid.NewGuid(), CancellationToken.None);

            events.Should().BeEmpty();
        }

        [Fact(DisplayName = "A02. Data can be stored")]
        public async Task StoringTest()
        {
            // Arrange
            var connection = new InMemorySqliteConnection();
            var connectionFactory = () => connection;
            var eventSerializer = new SystemTextJsonEventSerializer();
            var aggregateIdSerializer = new GuidAggregateIdSerializer();

            var aggregate = new BasicAggregate(Guid.NewGuid());
            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            var eventStore = new EventStore<Guid>(connectionFactory, eventSerializer, new GuidAggregateIdSerializer());

            // Act
            await eventStore.StoreAsync(aggregate.UncommittedEvents, CancellationToken.None);

            // Assert by querying the DB manually
            const string sql = @"
                SELECT
                    DateTime, EventId, EventName, AggregateId, AggregateName, EventIndex, Payload
                FROM Events
                WHERE
                    AggregateId = @AggregateId
                ORDER BY
                    EventIndex ASC";
            var parameters = new { AggregateId = aggregateIdSerializer.Serialize(aggregate.AggregateId) };
            var command = new CommandDefinition(sql, parameters: parameters);
            var results = await connection.QueryAsync<EventStore<Guid>.EventData>(command);

            results.Should().SatisfyRespectively(
                jeden => {
                    jeden.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("jeden")));
                    DateTimeOffset.Parse(jeden.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now1);
                    jeden.EventId.Should().NotBeEmpty();
                    jeden.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    jeden.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                    jeden.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    jeden.EventIndex.Should().Be(1);
                },
                dwa => {
                    dwa.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("dwa")));
                    DateTimeOffset.Parse(dwa.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now2);
                    dwa.EventId.Should().NotBeEmpty();
                    dwa.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    dwa.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                    dwa.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    dwa.EventIndex.Should().Be(2);
                },
                trzy => {
                    trzy.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("trzy")));
                    DateTimeOffset.Parse(trzy.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now3);
                    trzy.EventId.Should().NotBeEmpty();
                    trzy.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    trzy.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                    trzy.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    trzy.EventIndex.Should().Be(3);
                });
        }

        [Fact(DisplayName = "A03. Loaded events should match stored ones")]
        public async Task LoadedEventsShouldMatchStoredOnes()
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

            var eventStore = new EventStore<Guid>(connectionFactory, eventSerializer, new GuidAggregateIdSerializer());

            await eventStore.StoreAsync(aggregate.UncommittedEvents, CancellationToken.None);

            // Act
            var events = await eventStore.LoadAsync(aggregate.AggregateId, CancellationToken.None);

            // Assert
            events.Should().SatisfyRespectively(
                jeden => {
                    jeden.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                        .Which.Value.Should().Be("jeden");
                    jeden.Metadata.DateTime.Should().Be(now1);
                    jeden.Metadata.EventId.Should().NotBeEmpty();
                    jeden.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    jeden.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                    jeden.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    jeden.Metadata.EventIndex.Should().Be(1);
                },
                dwa => {
                    dwa.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                        .Which.Value.Should().Be("dwa");
                    dwa.Metadata.DateTime.Should().Be(now2);
                    dwa.Metadata.EventId.Should().NotBeEmpty();
                    dwa.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    dwa.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                    dwa.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    dwa.Metadata.EventIndex.Should().Be(2);
                },
                trzy => {
                    trzy.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                        .Which.Value.Should().Be("trzy");
                    trzy.Metadata.DateTime.Should().Be(now3);
                    trzy.Metadata.EventId.Should().NotBeEmpty();
                    trzy.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                    trzy.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                    trzy.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                    trzy.Metadata.EventIndex.Should().Be(3);
                });
        }
    }
}