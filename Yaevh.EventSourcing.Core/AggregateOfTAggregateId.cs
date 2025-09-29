using System.Collections.Immutable;

namespace Yaevh.EventSourcing.Core
{
    public abstract class Aggregate<TAggregate, TAggregateId> : IAggregate<TAggregateId>
        where TAggregate: Aggregate<TAggregate, TAggregateId>
        where TAggregateId : notnull
    {
        public TAggregateId AggregateId { get; }

        public long Version { get; private set; } = 0;
        public bool IsTransient => Version == 0;


        private readonly List<AggregateEvent<TAggregateId>> _committedEvents = new();
        public IReadOnlyList<AggregateEvent<TAggregateId>> CommittedEvents => _committedEvents.AsReadOnly();

        private readonly List<AggregateEvent<TAggregateId>> _uncommittedEvents = [];
        public IReadOnlyList<AggregateEvent<TAggregateId>> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        public IReadOnlyCollection<AggregateEvent<TAggregateId>> AllEvents
            => _committedEvents.Concat(_uncommittedEvents).ToImmutableList();

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
                _committedEvents.Add(@event);
            }
        }

        // TODO change to AggregateEvent<TAggregateId>
        protected abstract void Apply(IEventPayload? aggregateEvent);


        public async Task<IReadOnlyList<AggregateEvent<TAggregateId>>> CommitAsync(IEventStore<TAggregateId> eventStore, CancellationToken cancellationToken)
        {
            var newEvents = UncommittedEvents.ToImmutableList();
            await eventStore.StoreAsync(newEvents, cancellationToken);
            _committedEvents.AddRange(newEvents);
            _uncommittedEvents.Clear();
            return newEvents;
        }
    }
}
