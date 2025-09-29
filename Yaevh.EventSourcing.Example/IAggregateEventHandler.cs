using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.Example;

public interface IAggregateEventHandler<TAggegate, TAggregateId, TEventPayload>
    where TAggegate : IAggregate<TAggregateId>
    where TAggregateId : notnull
    where TEventPayload : IEventPayload
{
    Task Handle(TAggegate aggegate, TEventPayload @event, CancellationToken cancellationToken);
}
