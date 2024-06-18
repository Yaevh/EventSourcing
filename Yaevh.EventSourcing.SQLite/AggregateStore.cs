using Ardalis.GuardClauses;
using Dapper;
using System.Collections.Concurrent;
using System.Data;
using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.SQLite
{
    public class AggregateStore : IAggregateStore
    {
        private readonly Func<IDbConnection> _dbConnectionFactory;
        private readonly IEventSerializer _eventSerializer;
        private readonly IReadOnlyDictionary<Type, IAggregateIdSerializer> _aggregateIdSerializers;
        private readonly ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>();
        public AggregateStore(
            Func<IDbConnection> dbConnectionFactory,
            IEventSerializer eventSerializer,
            IReadOnlyDictionary<Type, IAggregateIdSerializer> aggregateIdSerializers)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
            _aggregateIdSerializers = aggregateIdSerializers ?? throw new ArgumentNullException(nameof(aggregateIdSerializers));
        }

        public async Task<IEnumerable<DomainEvent<TAggregateId>>> LoadAsync<TAggregateId>(
            TAggregateId aggregateId, CancellationToken cancellationToken)
            where TAggregateId : notnull
        {
            Guard.Against.Null(aggregateId);

            await EnsureDatabase(cancellationToken);

            var aggregateIdSerializer = (IAggregateIdSerializer<TAggregateId>)_aggregateIdSerializers[typeof(TAggregateId)];

            using (var connection = _dbConnectionFactory.Invoke())
            {
                const string sql = @"
                    SELECT
                        DateTime, EventId, EventName, AggregateId, AggregateName, EventIndex, Data
                    FROM Events
                    WHERE
                        AggregateId = @AggregateId
                    ORDER BY
                        EventIndex ASC";
                var parameters = new { AggregateId = aggregateIdSerializer.Serialize(aggregateId) };
                var command = new CommandDefinition(sql, parameters: parameters, cancellationToken: cancellationToken);
                var results = await connection.QueryAsync<EventData>(command);
                return results.Select(x => ParseToDomainEvent<TAggregateId>(x));
            }
        }

        public async Task StoreAsync<TAggregate, TAggregateId>(
            TAggregate aggregate, IReadOnlyList<DomainEvent<TAggregateId>> events, CancellationToken cancellationToken)
            where TAggregate : notnull, IAggregate<TAggregateId>
            where TAggregateId : notnull
        {
            Guard.Against.Null(aggregate);
            Guard.Against.Null(events);

            await EnsureDatabase(cancellationToken);

            var aggregateIdSerializer = (IAggregateIdSerializer<TAggregateId>)_aggregateIdSerializers[typeof(TAggregateId)];

            const string sql = @"
                INSERT INTO
                    Events
                        (DateTime, EventId, EventName, AggregateId, AggregateName, EventIndex, Data)
                    VALUES
                        (@DateTime, @EventId, @EventName, @AggregateId, @AggregateName, @EventIndex, @Data);
                SELECT last_insert_rowid() FROM Events";

            using (var connection = _dbConnectionFactory.Invoke())
            {
                foreach (var @event in events)
                {
                    var parameters = new {
                        DateTime = @event.Metadata.DateTime,
                        EventId = @event.Metadata.EventId,
                        EventName = @event.Metadata.EventName,
                        AggregateId = aggregateIdSerializer.Serialize(@event.Metadata.AggregateId),
                        AggregateName = @event.Metadata.AggregateName,
                        EventIndex = @event.Metadata.EventIndex,
                        Data = _eventSerializer.Serialize(@event.Data)
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
                        Data TEXT NOT NULL
                    )";
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }

        private DomainEvent<TAggregateId> ParseToDomainEvent<TAggregateId>(EventData source)
            where TAggregateId : notnull
        {
            var aggregateIdSerializer = (IAggregateIdSerializer<TAggregateId>)_aggregateIdSerializers[typeof(TAggregateId)];

            var metadata = new DefaultEventMetadata<TAggregateId>(
                DateTimeOffset.Parse(source.DateTime, System.Globalization.CultureInfo.InvariantCulture),
                Guid.Parse(source.EventId),
                source.EventName,
                aggregateIdSerializer.Deserialize(source.AggregateId),
                source.AggregateName,
                source.EventIndex);

            var type = _typeCache.GetOrAdd(metadata.EventName, typeName => Type.GetType(typeName, throwOnError: true)!);

            var @event = _eventSerializer.Deserialize(source.Data, type) as IEvent;

            return new DomainEvent<TAggregateId>(@event!, metadata);
        }


        internal record EventData(
            string DateTime,
            string EventId,
            string EventName,
            string AggregateId,
            string AggregateName,
            long EventIndex,
            string Data);
    }
}