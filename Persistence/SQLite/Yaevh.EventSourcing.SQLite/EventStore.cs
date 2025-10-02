using Dapper;
using System.Collections.Concurrent;
using System.Data;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.SQLite
{
    public class EventStore<TAggregateId> : IEventStore<TAggregateId>
        where TAggregateId : notnull
    {
        private static readonly ConcurrentDictionary<string, Type> _eventTypeCache = new();
        private static readonly ConcurrentDictionary<string, Type> _metadataTypeCache = new();
        private bool _isDatabaseEnsured = false;

        private readonly Func<IDbConnection> _dbConnectionFactory;
        private readonly IEventSerializer _eventSerializer;
        private readonly IAggregateIdSerializer<TAggregateId> _aggregateIdSerializer;
        public EventStore(
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
                        AggregateName, AggregateId, EventName, EventId, EventIndex, DateTime, Payload, MetadataType, Metadata
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

        public async Task StoreAsync(
            IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(events);

            await EnsureDatabase(cancellationToken);

            const string sql = @"
                INSERT INTO
                    Events
                        ( AggregateName,  AggregateId,  EventName,  EventId,  EventIndex,  DateTime,  Payload,  MetadataType,  Metadata)
                    VALUES
                        (@AggregateName, @AggregateId, @EventName, @EventId, @EventIndex, @DateTime, @Payload, @MetadataType, @Metadata);
                SELECT last_insert_rowid() FROM Events";

            using (var connection = _dbConnectionFactory.Invoke())
            {
                foreach (var @event in events)
                {
                    var parameters = new {
                        AggregateName = @event.AggregateName,
                        AggregateId = _aggregateIdSerializer.Serialize(@event.AggregateId),
                        EventName = @event.EventName,
                        EventId = @event.EventId,
                        EventIndex = @event.EventIndex,
                        DateTime = @event.DateTime,
                        Payload = _eventSerializer.Serialize(@event.Payload),
                        MetadataType = (string?)null,
                        Metadata = (string?)null
                    };
                    if (@event is AggregateEventWithMetadata<TAggregateId> withMetadata)
                        parameters = parameters with {
                            MetadataType = withMetadata.Metadata.GetType().AssemblyQualifiedName,
                            Metadata = _eventSerializer.Serialize(withMetadata.Metadata)
                        };

                    var command = new CommandDefinition(sql, parameters: parameters, cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(command);
                }
            }
        }


        public async Task<IEnumerable<TAggregateId>> GetAllAggregateIdsAsync(CancellationToken cancellationToken)
        {
            using (var connection = _dbConnectionFactory.Invoke())
            {
                const string sql = @"SELECT DISTINCT AggregateId FROM Events";
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                var results = await connection.QueryAsync<string>(command);
                return results.Select(aggregateId => _aggregateIdSerializer.Deserialize(aggregateId));
            }
        }

        private async Task EnsureDatabase(CancellationToken cancellationToken)
        {
            if (_isDatabaseEnsured)
                return;

            using (var connection = _dbConnectionFactory.Invoke())
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS Events (
                        AggregateName TEXT NOT NULL,
                        AggregateId TEXT NOT NULL,
                        EventName TEXT NOT NULL,
                        EventId TEXT PRIMARY KEY NOT NULL,
                        EventIndex INT NOT NULL,
                        DateTime TEXT NOT NULL,
                        Payload TEXT NOT NULL,
                        MetadataType TEXT NULL,
                        Metadata TEXT NULL,
                        UNIQUE(AggregateId, EventIndex)
                    );
                    CREATE INDEX IF NOT EXISTS idx_Events_AggregateId ON Events(AggregateId);";
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);

                _isDatabaseEnsured = true;
            }
        }

        private AggregateEvent<TAggregateId> ParseToDomainEvent(EventData source)
        {
            var eventType = _eventTypeCache.GetOrAdd(source.EventName, typeName => Type.GetType(typeName, throwOnError: true)!);
            var @event = _eventSerializer.Deserialize(source.Payload, eventType) as IEventPayload;

            var aggregateEvent = new AggregateEvent<TAggregateId>(
                @event!,
                source.AggregateName,
                _aggregateIdSerializer.Deserialize(source.AggregateId),
                source.EventName,
                Guid.Parse(source.EventId),
                source.EventIndex,
                DateTimeOffset.Parse(source.DateTime, System.Globalization.CultureInfo.InvariantCulture));

            if (source.MetadataType == null)
                return aggregateEvent;

            var metadataType = _metadataTypeCache.GetOrAdd(source.MetadataType, typeName => Type.GetType(typeName, throwOnError: true)!);
            var metadata = _eventSerializer.Deserialize(source.Metadata, metadataType);

            return new AggregateEventWithMetadata<TAggregateId>(aggregateEvent, source.MetadataType, metadata!);
        }


        internal record EventData(
            string AggregateName,
            string AggregateId,
            string EventName,
            string EventId,
            long EventIndex,
            string DateTime,
            string Payload,
            string MetadataType,
            string Metadata);
    }
}