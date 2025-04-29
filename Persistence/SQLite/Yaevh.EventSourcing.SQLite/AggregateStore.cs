using Dapper;
using System.Collections.Concurrent;
using System.Data;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.SQLite
{
    public class AggregateStore<TAggregateId> : IAggregateStore<TAggregateId>
        where TAggregateId : notnull
    {
        private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

        private readonly Func<IDbConnection> _dbConnectionFactory;
        private readonly IEventSerializer _eventSerializer;
        private readonly IAggregateIdSerializer<TAggregateId> _aggregateIdSerializer;
        public AggregateStore(
            Func<IDbConnection> dbConnectionFactory,
            IEventSerializer eventSerializer,
            IAggregateIdSerializer<TAggregateId> aggregateIdSerializer)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
            _aggregateIdSerializer = aggregateIdSerializer ?? throw new ArgumentNullException(nameof(aggregateIdSerializer));
        }

        public async Task<IEnumerable<AggregateEvent<TAggregateId>>> LoadAsync(
            TAggregateId aggregateId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(aggregateId);

            await EnsureDatabase(cancellationToken);

            using (var connection = _dbConnectionFactory.Invoke())
            {
                const string sql = @"
                    SELECT
                        DateTime, EventId, EventName, AggregateId, AggregateName, EventIndex, Payload
                    FROM Events
                    WHERE
                        AggregateId = @AggregateId
                    ORDER BY
                        EventIndex ASC";
                var parameters = new { AggregateId = _aggregateIdSerializer.Serialize(aggregateId) };
                var command = new CommandDefinition(sql, parameters: parameters, cancellationToken: cancellationToken);
                var results = await connection.QueryAsync<EventData>(command);
                return results.Select(eventData => ParseToDomainEvent(eventData));
            }
        }

        public async Task StoreAsync<TAggregate>(
            TAggregate aggregate, IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
            where TAggregate : notnull, IAggregate<TAggregateId>
        {
            ArgumentNullException.ThrowIfNull(aggregate);
            ArgumentNullException.ThrowIfNull(events);

            await EnsureDatabase(cancellationToken);

            const string sql = @"
                INSERT INTO
                    Events
                        (DateTime, EventId, EventName, AggregateId, AggregateName, EventIndex, Payload)
                    VALUES
                        (@DateTime, @EventId, @EventName, @AggregateId, @AggregateName, @EventIndex, @Payload);
                SELECT last_insert_rowid() FROM Events";

            using (var connection = _dbConnectionFactory.Invoke())
            {
                foreach (var @event in events)
                {
                    var parameters = new {
                        DateTime = @event.Metadata.DateTime,
                        EventId = @event.Metadata.EventId,
                        EventName = @event.Metadata.EventName,
                        AggregateId = _aggregateIdSerializer.Serialize(@event.Metadata.AggregateId),
                        AggregateName = @event.Metadata.AggregateName,
                        EventIndex = @event.Metadata.EventIndex,
                        Payload = _eventSerializer.Serialize(@event.Payload)
                    };
                    var command = new CommandDefinition(sql, parameters: parameters, cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(command);
                }
            }
        }


        private async Task EnsureDatabase(CancellationToken cancellationToken)
        {
            using (var connection = _dbConnectionFactory.Invoke())
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS Events (
                        DateTime TEXT NOT NULL,
                        EventId TEXT PRIMARY KEY NOT NULL,
                        EventName TEXT NOT NULL,
                        AggregateId TEXT NOT NULL,
                        AggregateName TEXT NOT NULL,
                        EventIndex INT NOT NULL,
                        Payload TEXT NOT NULL
                    )";
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }

        private AggregateEvent<TAggregateId> ParseToDomainEvent(EventData source)
        {
            var metadata = new DefaultEventMetadata<TAggregateId>(
                DateTimeOffset.Parse(source.DateTime, System.Globalization.CultureInfo.InvariantCulture),
                Guid.Parse(source.EventId),
                source.EventName,
                _aggregateIdSerializer.Deserialize(source.AggregateId),
                source.AggregateName,
                source.EventIndex);

            var type = _typeCache.GetOrAdd(metadata.EventName, typeName => Type.GetType(typeName, throwOnError: true)!);

            var @event = _eventSerializer.Deserialize(source.Payload, type) as IEventPayload;

            return new AggregateEvent<TAggregateId>(@event, metadata);
        }


        internal record EventData(
            string DateTime,
            string EventId,
            string EventName,
            string AggregateId,
            string AggregateName,
            long EventIndex,
            string Payload);
    }
}