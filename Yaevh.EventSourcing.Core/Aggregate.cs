using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public abstract class Aggregate<TAggregate> : IAggregate<Guid>
        where TAggregate: Aggregate<TAggregate>
    {
        public Guid AggregateId { get; }

        public long Version { get; private set; } = 0;


        private List<DomainEvent<Guid>> _uncommittedEvents = new List<DomainEvent<Guid>>();
        public IReadOnlyList<DomainEvent<Guid>> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        protected Aggregate(Guid aggregateId)
        {
            Guard.Against.Default(aggregateId);
            AggregateId = aggregateId;
        }


        protected void RaiseEvent(IEvent @event, DateTimeOffset dateTime)
        {
            var metadata = DefaultEventMetadata<Guid>.Create(this, @event, dateTime);
            RaiseEvent(@event, metadata);
        }

        protected void RaiseEvent(IEvent @event, IEventMetadata<Guid> metadata)
        {
            _uncommittedEvents.Add(new DomainEvent<Guid>(@event, metadata));
            ++Version;
            Apply(@event);
        }

        public void Load(IEnumerable<DomainEvent<Guid>> events)
        {
            foreach (var @event in events)
            {
                ++Version;
                Apply(@event.Data);
            }
        }

        protected abstract void Apply(IEvent aggregateEvent);


        public async Task<IReadOnlyList<DomainEvent<Guid>>> CommitAsync(IAggregateStore aggregateStore, CancellationToken cancellationToken)
        {
            var newEvents = UncommittedEvents.ToImmutableList();
            await aggregateStore.StoreAsync(this, newEvents, cancellationToken);
            _uncommittedEvents.Clear();
            return newEvents;
        }
    }
}
