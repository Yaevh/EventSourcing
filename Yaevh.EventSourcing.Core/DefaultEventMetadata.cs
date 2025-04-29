using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public record DefaultEventMetadata<TAggregateId> : IEventMetadata<TAggregateId>
        where TAggregateId : notnull
    {
        public DateTimeOffset DateTime { get; }

        public Guid EventId { get; }
        public string EventName { get; }
        public TAggregateId AggregateId { get; }
        public string AggregateName { get; }
        public long EventIndex { get; }

        public DefaultEventMetadata(
            DateTimeOffset dateTime,
            Guid eventId,
            string eventName,
            TAggregateId aggregateId,
            string aggregateName,
            long eventIndex)
        {
            Guard.Against.Default(eventId);
            Guard.Against.NullOrWhiteSpace(eventName);
            Guard.Against.Default(dateTime);
            Guard.Against.Default(aggregateId);
            Guard.Against.NullOrWhiteSpace(aggregateName);
            Guard.Against.Default(eventIndex);

            EventId = eventId;
            EventName = eventName;
            DateTime = dateTime;
            AggregateId = aggregateId;
            AggregateName = aggregateName;
            EventIndex = eventIndex;
        }

        public static DefaultEventMetadata<TAggregateId> Create<TAggregate>(
            TAggregate aggregate, IEventPayload @event, DateTimeOffset dateTime)
            where TAggregate : IAggregate<TAggregateId>
        {
            Guard.Against.Default(aggregate);
            Guard.Against.Default(@event);
            Guard.Against.Default(dateTime);
            return new DefaultEventMetadata<TAggregateId>(
                dateTime,
                Guid.NewGuid(),
                @event.GetType().AssemblyQualifiedName!,
                aggregate.AggregateId,
                aggregate.GetType().AssemblyQualifiedName!,
                aggregate.Version + 1);
        }
    }
}
