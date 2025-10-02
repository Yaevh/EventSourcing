namespace Yaevh.EventSourcing;

/// <summary>
/// An event raised by an <see cref="IAggregate{TAggregateId}"/>, complete with its metadata
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
/// <param name="Payload">The domain-specific data for the event.</param>
/// <param name="DateTime">The date and time the event was raised.</param>
/// <param name="AggregateName">The name of the aggregate this event belongs to.</param>
/// <param name="AggregateId">Uniquely identifies the aggregate the event belongs to.</param>
/// <param name="EventName">The name of the raised event.</param>
/// <param name="EventId">Uniquely identifies the event this metadata belongs to.</param>
/// <param name="EventIndex">The one-based index of the event with regard to the aggregate. That is, the ordinal number of the event in a given aggregate.</param>
public record AggregateEvent<TAggregateId>(
    IEventPayload Payload,
    string AggregateName,
    TAggregateId AggregateId,
    string EventName,
    Guid EventId,
    long EventIndex,
    DateTimeOffset DateTime
)
    where TAggregateId : notnull
{
    public AggregateEvent(IAggregate<TAggregateId> aggregate, Guid eventId, IEventPayload payload, DateTimeOffset dateTime)
        : this(
            payload,
            aggregate.GetType().AssemblyQualifiedName!,
            aggregate.AggregateId,
            payload?.GetType().AssemblyQualifiedName!,
            eventId,
            aggregate.Version + 1,
            dateTime)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (dateTime == default) throw new ArgumentException("Value cannot be the default DateTimeOffset.", nameof(dateTime));
    }
}
