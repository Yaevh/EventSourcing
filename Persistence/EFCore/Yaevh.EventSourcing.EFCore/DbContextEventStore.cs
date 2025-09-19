using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Data;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore;

/// <summary>
/// An EventStore that uses an <see cref="EventsDbContext{TAggregateId}"/> to store aggregate events
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TAggregateId"></typeparam>
public class DbContextEventStore<TDbContext, TAggregateId> : IEventStore<TAggregateId>
    where TDbContext : EventsDbContext<TAggregateId>
    where TAggregateId : notnull
{
    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

    private readonly TDbContext _dbContext;
    private readonly IEventSerializer _eventSerializer;
    public DbContextEventStore(
        TDbContext dbContext,
        IEventSerializer eventSerializer)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    public async Task<IEnumerable<AggregateEvent<TAggregateId>>> LoadAsync(
        TAggregateId aggregateId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        var eventDatas = await _dbContext.Events
            .AsNoTracking()
            .OrderBy(x => x.EventIndex)
            .Where(e => e.AggregateId.Equals(aggregateId))
            .ToListAsync(cancellationToken);

        return eventDatas.Select(eventData => ToAggregateEvent(eventData));
    }

    public async Task StoreAsync<TAggregate>(
        TAggregate aggregate, IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
        where TAggregate: IAggregate<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(events);

        await _dbContext.Events.AddRangeAsync(events.Select(@event => ToEventData(@event)), cancellationToken);
    }

    internal AggregateEvent<TAggregateId> ToAggregateEvent(EventData<TAggregateId> source)
    {
        var eventType = _typeCache.GetOrAdd(source.EventName, typeName => Type.GetType(typeName, throwOnError: true)!);

        var metadata = new DefaultEventMetadata<TAggregateId>(
            source.DateTime,
            source.EventId,
            source.EventName,
            source.AggregateId,
            source.AggregateName,
            source.EventIndex);

        var @event = _eventSerializer.Deserialize(source.Payload, eventType) as IEventPayload;

        return new AggregateEvent<TAggregateId>(@event, metadata);
    }

    internal EventData<TAggregateId> ToEventData(AggregateEvent<TAggregateId> source)
    {
        var payload = _eventSerializer.Serialize(source.Payload);

        return new EventData<TAggregateId>(
            source.Metadata.EventId,
            source.Metadata.DateTime.ToUniversalTime(),
            source.Metadata.EventName,
            source.Metadata.AggregateId,
            source.Metadata.AggregateName,
            source.Metadata.EventIndex,
            payload);
    }

}