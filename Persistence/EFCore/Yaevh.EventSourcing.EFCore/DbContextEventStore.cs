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
    private static readonly ConcurrentDictionary<string, Type> _metadataTypeCache = new();

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

    public async Task StoreAsync(
        IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        await StoreWithoutSavingAsync(events, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task StoreWithoutSavingAsync(
        IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        await _dbContext.Events.AddRangeAsync(events.Select(@event => ToEventData(@event)), cancellationToken);
    }

    public async Task<IEnumerable<TAggregateId>> GetAllAggregateIdsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Events.Select(e => e.AggregateId).Distinct().ToListAsync(cancellationToken);
    }

    internal AggregateEvent<TAggregateId> ToAggregateEvent(EventData<TAggregateId> source)
    {
        var eventType = _typeCache.GetOrAdd(source.EventName, typeName => Type.GetType(typeName, throwOnError: true)!);

        var @event = _eventSerializer.Deserialize(source.Payload, eventType) as IEventPayload;

        var aggregateEvent = new AggregateEvent<TAggregateId>(
            @event!,
            source.AggregateName,
            source.AggregateId,
            source.EventName,
            source.EventId,
            source.EventIndex,
            source.DateTime.ToLocalTime());

        if (source.MetadataType == null)
            return aggregateEvent;

        var metadataType = _metadataTypeCache.GetOrAdd(source.MetadataType, typeName => Type.GetType(typeName, throwOnError: true)!);
        var metadata = _eventSerializer.Deserialize(source.Metadata, metadataType);

        return new AggregateEventWithMetadata<TAggregateId>(aggregateEvent, source.MetadataType, metadata!);
    }

    internal EventData<TAggregateId> ToEventData(AggregateEvent<TAggregateId> source)
    {
        var payload = _eventSerializer.Serialize(source.Payload);

        var eventData = new EventData<TAggregateId>(
            source.AggregateName,
            source.AggregateId,
            source.EventName,
            source.EventId,
            source.EventIndex,
            source.DateTime.ToUniversalTime(),
            payload,
            null, null);
        if (source is AggregateEventWithMetadata<TAggregateId> withMetadata)
            eventData = eventData with {
                MetadataType = withMetadata.MetadataType,
                Metadata = _eventSerializer.Serialize(withMetadata.Metadata)
            };

        return eventData;
    }

}