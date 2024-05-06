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
    public interface IAggregateStore
    {
        /// <summary>
        /// Load the latest version of an aggregate.
        /// </summary>
        /// <typeparam name="TAggregateId"></typeparam>
        /// <param name="aggregateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<DomainEvent<TAggregateId>>>
            LoadAsync<TAggregateId>(TAggregateId aggregateId, CancellationToken cancellationToken);

        /// <summary>
        /// Save the aggregate and its events to a persistent storage.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <typeparam name="TAggregateId"></typeparam>
        /// <param name="aggregate"></param>
        /// <param name="events"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StoreAsync<TAggregate, TAggregateId>(
            TAggregate aggregate, IReadOnlyList<DomainEvent<TAggregateId>> events, CancellationToken cancellationToken)
            where TAggregate : IAggregate<TAggregateId>;
    }
}
