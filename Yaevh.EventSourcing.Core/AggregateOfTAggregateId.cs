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


        private readonly List<AggregateEvent<TAggregateId>> _uncommittedEvents = [];
        public IReadOnlyList<AggregateEvent<TAggregateId>> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        protected Aggregate(TAggregateId aggregateId)
        {
            ArgumentNullException.ThrowIfNull(aggregateId);
            AggregateId = aggregateId;
        }


        protected void RaiseEvent(IEventPayload @event, DateTimeOffset dateTime)
        {
            var metadata = DefaultEventMetadata<TAggregateId>.Create(this, @event, dateTime);
            RaiseEvent(@event, metadata);
        }

        protected void RaiseEvent(IEventPayload @event, IEventMetadata<TAggregateId> metadata)
        {
            _uncommittedEvents.Add(new AggregateEvent<TAggregateId>(@event, metadata));
            ++Version;
            Apply(@event);
        }

        public void Load(IEnumerable<AggregateEvent<TAggregateId>> events)
        {
            foreach (var @event in events)
            {
                if (@event.Metadata.EventIndex != Version + 1)
                    throw new InvalidOperationException($"Event index {@event.Metadata.EventIndex} is out of order. Current version is {Version}.");
                ++Version;
                Apply(@event.Payload);
            }
        }

        protected abstract void Apply(IEventPayload? aggregateEvent);


        public async Task<IReadOnlyList<AggregateEvent<TAggregateId>>> CommitAsync(IAggregateStore<TAggregateId> aggregateStore, CancellationToken cancellationToken)
        {
            var newEvents = UncommittedEvents.ToImmutableList();
            await aggregateStore.StoreAsync(this, newEvents, cancellationToken);
            _uncommittedEvents.Clear();
            return newEvents;
        }
    }
}
