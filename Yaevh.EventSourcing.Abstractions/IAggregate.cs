using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// The base entity; all aggregates should implement this
    /// </summary>
    /// <typeparam name="TAggregateId"></typeparam>
    public interface IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        TAggregateId AggregateId { get; }

        /// <summary>
        /// Represents the <see cref="IEventMetadata{TAggregateId}.EventIndex"/>
        /// of the latest <see cref="AggregateEvent{TAggregateId}"/> applied to this Aggregate
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the list of events that have been committed to the EventStore.
        /// </summary>
        IReadOnlyList<AggregateEvent<TAggregateId>> CommittedEvents { get; }

        /// <summary>
        /// Gets the list of events that haven't yet been committed to the EventStore.
        /// </summary>
        IReadOnlyList<AggregateEvent<TAggregateId>> UncommittedEvents { get; }

        IReadOnlyCollection<AggregateEvent<TAggregateId>> AllEvents
            => CommittedEvents.Concat(UncommittedEvents).ToImmutableList();


        /// <summary>
        /// Restores the state of the aggregate based on given events
        /// </summary>
        /// <param name="events"></param>
        void Load(IEnumerable<AggregateEvent<TAggregateId>> events);

        /// <summary>
        /// Stores the state of the aggregate in a given <see cref="IEventStore"/>
        /// </summary>
        /// <param name="eventStore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyList<AggregateEvent<TAggregateId>>> CommitAsync(IEventStore<TAggregateId> eventStore, CancellationToken cancellationToken);
    }
}
