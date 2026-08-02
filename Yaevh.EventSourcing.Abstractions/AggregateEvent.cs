namespace Yaevh.EventSourcing;

/// <summary>
/// An event raised by an <see cref="IAggregate{TAggregateId}"/>, complete with its metadata.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
/// <param name="Payload">The domain-specific data for the event.</param>
/// <param name="DateTime">The date and time the event was raised.</param>
/// <param name="AggregateType">The CLR type of the aggregate this event belongs to.</param>
/// <param name="AggregateId">Uniquely identifies the aggregate the event belongs to.</param>
/// <param name="EventName">The name of the raised event.</param>
/// <param name="EventId">Uniquely identifies the event this metadata belongs to.</param>
/// <param name="EventIndex">The one-based index of the event with regard to the aggregate. That is, the ordinal number of the event in a given aggregate.</param>
// TODO change EventName and EventMetadataName
public record AggregateEvent<TAggregateId>(
    IEventPayload Payload,
    Type AggregateType,
    TAggregateId AggregateId,
    string EventName,
    long EventId,
    long EventIndex,
    DateTimeOffset DateTime
)
    where TAggregateId : notnull
{
    public AggregateEvent(IAggregate<TAggregateId> aggregate, IEventPayload payload, DateTimeOffset dateTime)
        : this(
            payload,
            aggregate.GetType(),
            aggregate.AggregateId,
            payload?.GetType().AssemblyQualifiedName!,
            default,
            aggregate.Version + 1,
            dateTime)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (dateTime == default) throw new ArgumentException("Value cannot be the default DateTimeOffset.", nameof(dateTime));
    }
}

public record AggregateEventWithMetadata<TAggregateId>(
    IEventPayload Payload,
    Type AggregateType,
    TAggregateId AggregateId,
    string EventName,
    long EventId,
    long EventIndex,
    DateTimeOffset DateTime,
    string MetadataType,
    object Metadata
) : AggregateEvent<TAggregateId>(Payload, AggregateType, AggregateId, EventName, EventId, EventIndex, DateTime)
    where TAggregateId : notnull
{
    public AggregateEventWithMetadata(IAggregate<TAggregateId> aggregate, IEventPayload payload, DateTimeOffset dateTime, object metadata)
        : this(
            payload,
            aggregate.GetType(),
            aggregate.AggregateId,
            payload?.GetType().AssemblyQualifiedName!,
            default,
            aggregate.Version + 1,
            dateTime,
            metadata.GetType().AssemblyQualifiedName!,
            metadata)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (dateTime == default) throw new ArgumentException("Value cannot be the default DateTimeOffset.", nameof(dateTime));
    }

    public AggregateEventWithMetadata(AggregateEvent<TAggregateId> @event, string metadataType, object metadata)
        : this(
            @event.Payload,
            @event.AggregateType,
            @event.AggregateId,
            @event.EventName,
            @event.EventId,
            @event.EventIndex,
            @event.DateTime,
            MetadataType: metadataType,
            Metadata: metadata)
    { }
}
