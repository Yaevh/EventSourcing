using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Loads and saves the aggregates of a certain type.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TAggregateId"></typeparam>
    public interface IAggregateManager<TAggregate, TAggregateId>
        where TAggregate: IAggregate<TAggregateId>
    {
        /// <summary>
        /// Load the aggregate from an underlying persistent storage
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TAggregate> LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken);

        /// <summary>
        /// Save the aggregate (that is, its events) to an underlying persistent storage
        /// </summary>
        /// <param name="aggregate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken);
    }
}
