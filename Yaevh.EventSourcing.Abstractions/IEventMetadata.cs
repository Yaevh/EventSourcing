using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Represents metadata about a certain event.
    /// </summary>
    /// <typeparam name="TAggregateId"></typeparam>
    public interface IEventMetadata<TAggregateId>
    {
        /// <summary>
        /// The date and time the event was raised.
        /// </summary>
        DateTimeOffset DateTime { get; }

        /// <summary>
        /// Uniquely identifies the event this metadata belongs to.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// The name of the raised event.
        /// </summary>
        string EventName { get; }
        
        /// <summary>
        /// Uniquely identifies the aggregate the event belongs to.
        /// </summary>
        TAggregateId AggregateId { get; }

        /// <summary>
        /// The name of the aggregate this event belongs to.
        /// </summary>
        string AggregateName { get; }

        /// <summary>
        /// The one-based index of the event with regard to the AggregateId.
        /// That is, the ordinal number of the event in a given aggregate.
        /// </summary>
        /// <remarks>
        /// Given an aggregate with multiple events,
        /// the first event will have an <see cref="EventIndex"/> = 1,
        /// the second will have an <see cref="EventIndex"/> = 2 and so on.
        /// </remarks>
        long EventIndex { get; }
    }
}
