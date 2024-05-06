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
        Task<TAggregate> LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken);

        Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken);
    }
}
