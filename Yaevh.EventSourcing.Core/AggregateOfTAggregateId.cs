using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public abstract class Aggregate<TAggregate, TAggregateId> : IAggregate<TAggregateId>
        where TAggregate: Aggregate<TAggregate>
        where TAggregateId : notnull

    {
        public TAggregateId AggregateId { get; }

        public long Version { get; private set; } = 0;


        private readonly List<DomainEvent<TAggregateId>> _uncommittedEvents = [];
        public IReadOnlyList<DomainEvent<TAggregateId>> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        protected Aggregate(TAggregateId aggregateId)
        {
            Guard.Against.Default(aggregateId);
            AggregateId = aggregateId;
        }


        protected void RaiseEvent(IEventPayload @event, DateTimeOffset dateTime)
        {
            var metadata = DefaultEventMetadata<TAggregateId>.Create(this, @event, dateTime);
            RaiseEvent(@event, metadata);
        }

        protected void RaiseEvent(IEventPayload @event, IEventMetadata<TAggregateId> metadata)
        {
            _uncommittedEvents.Add(new DomainEvent<TAggregateId>(@event, metadata));
            ++Version;
            Apply(@event);
        }

        public void Load(IEnumerable<DomainEvent<TAggregateId>> events)
        {
            foreach (var @event in events)
            {
                ++Version;
                Apply(@event.Payload);
            }
        }

        protected abstract void Apply(IEventPayload aggregateEvent);


        public async Task<IReadOnlyList<DomainEvent<TAggregateId>>> CommitAsync(IAggregateStore aggregateStore, CancellationToken cancellationToken)
        {
            var newEvents = UncommittedEvents.ToImmutableList();
            await aggregateStore.StoreAsync(this, newEvents, cancellationToken);
            _uncommittedEvents.Clear();
            return newEvents;
        }
    }
}
