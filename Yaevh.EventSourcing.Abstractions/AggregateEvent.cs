namespace Yaevh.EventSourcing;

/// <summary>
/// An event raised by an <see cref="IAggregate{TAggregateId}"/>, complete with its metadata
/// </summary>
/// <typeparam name="TAggregateId"></typeparam>
/// <param name="Payload"></param>
/// <param name="Metadata"></param>
public class AggregateEvent<TAggregateId>(
    IEventPayload? Payload,
    IEventMetadata<TAggregateId> Metadata
)
    where TAggregateId : notnull
{
    public IEventPayload? Payload { get; } = Payload;
    public IEventMetadata<TAggregateId> Metadata { get; } = Metadata;
}

public class AggregateEvent<TAggregateId, TPayload>(
    TPayload? Payload,
    IEventMetadata<TAggregateId> Metadata
) : AggregateEvent<TAggregateId>(Payload, Metadata)
    where TAggregateId : notnull
    where TPayload : IEventPayload
{
    public new TPayload? Payload { get; } = Payload;
}


