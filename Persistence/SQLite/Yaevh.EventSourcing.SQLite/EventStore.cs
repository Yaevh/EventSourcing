using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using Dapper;
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
        private readonly IAggregateTypeNamingStrategy _aggregateNamingStrategy;
        public EventStore(
            Func<IDbConnection> dbConnectionFactory,
            IEventSerializer eventSerializer,
            IAggregateIdSerializer<TAggregateId> aggregateIdSerializer,
            IAggregateTypeNamingStrategy aggregateNamingStrategy)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
            _aggregateIdSerializer = aggregateIdSerializer ?? throw new ArgumentNullException(nameof(aggregateIdSerializer));
            _aggregateNamingStrategy = aggregateNamingStrategy ?? throw new ArgumentNullException(nameof(aggregateNamingStrategy));
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
                        AggregateType, AggregateId, EventName, EventId, EventIndex, DateTime, Payload, MetadataType, Metadata
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
                        ( AggregateType,  AggregateId,  EventName,  EventIndex,  DateTime,  Payload,  MetadataType,  Metadata)
                    VALUES
                        (@AggregateType, @AggregateId, @EventName, @EventIndex, @DateTime, @Payload, @MetadataType, @Metadata);
                SELECT last_insert_rowid() FROM Events";

            using var connection = _dbConnectionFactory.Invoke();
            IDbTransaction? transaction = null;
            try
            {
                transaction = connection.BeginTransaction();

                var parameters = events
                    .Select(@event => {
                        var parameters = new {
                            AggregateType =  _aggregateNamingStrategy.ToUniqueName(@event.AggregateType),
                            AggregateId = _aggregateIdSerializer.Serialize(@event.AggregateId),
                            EventName = @event.EventName,
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
                        return parameters;
                    })
                    .ToList();

                var command = new CommandDefinition(
                    sql, parameters: parameters, transaction: transaction, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);

                transaction.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
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
                        AggregateType TEXT NOT NULL,
                        AggregateId TEXT NOT NULL,
                        EventName TEXT NOT NULL,
                        EventId INTEGER PRIMARY KEY,
                        EventIndex INT NOT NULL,
                        DateTime TEXT NOT NULL,
                        Payload TEXT NOT NULL,
                        MetadataType TEXT NULL,
                        Metadata TEXT NULL,
                        UNIQUE (AggregateId, EventIndex)
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
                _aggregateNamingStrategy.FromUniqueName(source.AggregateType),
                _aggregateIdSerializer.Deserialize(source.AggregateId),
                source.EventName,
                source.EventId,
                source.EventIndex,
                DateTimeOffset.Parse(source.DateTime, CultureInfo.InvariantCulture));

            if (source.MetadataType == null)
                return aggregateEvent;

            var metadataType = _metadataTypeCache.GetOrAdd(source.MetadataType, typeName => Type.GetType(typeName, throwOnError: true)!);
            var metadata = _eventSerializer.Deserialize(source.Metadata, metadataType);

            return new AggregateEventWithMetadata<TAggregateId>(aggregateEvent, source.MetadataType!, metadata!);
        }


        internal record EventData(
            string AggregateType,
            string AggregateId,
            string EventName,
            long EventId,
            long EventIndex,
            string DateTime,
            string Payload,
            string MetadataType,
            string Metadata);
    }
}