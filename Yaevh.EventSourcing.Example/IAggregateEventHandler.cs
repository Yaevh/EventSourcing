using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.Example;

public interface IAggregateEventHandler<TAggregate, TAggregateId, TEventPayload>
    where TAggregate : IAggregate<TAggregateId>
    where TAggregateId : notnull
    where TEventPayload : IEventPayload
{
    Task Handle(TAggregate aggregate, TEventPayload @event, CancellationToken cancellationToken);
}
