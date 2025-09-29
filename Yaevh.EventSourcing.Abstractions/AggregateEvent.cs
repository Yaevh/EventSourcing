namespace Yaevh.EventSourcing;

/// <summary>
/// An event raised by an <see cref="IAggregate{TAggregateId}"/>, complete with its metadata
/// </summary>
/// <typeparam name="TAggregateId"></typeparam>
/// <param name="Payload"></param>
/// <param name="Metadata"></param>
public record AggregateEvent<TAggregateId>(
    IEventPayload? Payload,
    IEventMetadata<TAggregateId> Metadata
) where TAggregateId : notnull;
