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
            if (eventId == default) throw new ArgumentException("Value cannot be the default GUID.", nameof(eventId));
            ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            if (dateTime == default) throw new ArgumentException("Value cannot be the default DateTimeOffset.", nameof(dateTime));
            if (aggregateId == null || aggregateId.Equals(default(TAggregateId))) throw new ArgumentException("Value cannot be the default value.", nameof(aggregateId));
            ArgumentException.ThrowIfNullOrWhiteSpace(aggregateName);
            if (eventIndex <= 0) throw new ArgumentOutOfRangeException(nameof(eventIndex), "Value cannot be negative or zero.");
            
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
            ArgumentNullException.ThrowIfNull(aggregate);
            ArgumentNullException.ThrowIfNull(@event);
            if (dateTime == default) throw new ArgumentException("Value cannot be the default DateTimeOffset.", nameof(dateTime));

            return new DefaultEventMetadata<TAggregateId>(
                dateTime,
                MassTransit.NewId.Next().ToSequentialGuid(),
                @event.GetType().AssemblyQualifiedName!,
                aggregate.AggregateId,
                aggregate.GetType().AssemblyQualifiedName!,
                aggregate.Version + 1);
        }
    }
}
