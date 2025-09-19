using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// A persistent store responsible for storing and retrieving the aggregate events.
    /// </summary>
    public interface IEventStore<TAggregateId>
        where TAggregateId : notnull
    {
        /// <summary>
        /// Load the latest version of an aggregate.
        /// </summary>
        /// <typeparam name="TAggregateId"></typeparam>
        /// <param name="aggregateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<AggregateEvent<TAggregateId>>>
            LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken);

        /// <summary>
        /// Save the aggregate and its events to a persistent storage.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <typeparam name="TAggregateId"></typeparam>
        /// <param name="aggregate"></param>
        /// <param name="events"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StoreAsync<TAggregate>(
            TAggregate aggregate, IReadOnlyList<AggregateEvent<TAggregateId>> events, CancellationToken cancellationToken)
            where TAggregate : notnull, IAggregate<TAggregateId>;
    }
}
