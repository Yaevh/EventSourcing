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
        /// Load the events associated with a particular aggregate from persistent storage.
        /// </summary>
        /// <typeparam name="TAggregateId"></typeparam>
        /// <param name="aggregateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<AggregateEvent<TAggregateId>>> LoadAsync(
            TAggregateId aggregateId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Save the aggregate's events to a persistent storage.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        /// <param name="events"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StoreAsync(
            IReadOnlyList<AggregateEvent<TAggregateId>> events,
            CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously retrieves all aggregate IDs.
        /// </summary>
        /// <remarks>The returned collection may be empty if no aggregate IDs are available. The order of
        /// the  aggregate IDs in the collection is not guaranteed.</remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.
        /// Passing a canceled token will immediately  throw an <see cref="OperationCanceledException"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="IEnumerable{T}"/>
        /// of aggregate IDs.</returns>
        Task<IEnumerable<TAggregateId>> GetAllAggregateIdsAsync(
            CancellationToken cancellationToken);
    }
}
