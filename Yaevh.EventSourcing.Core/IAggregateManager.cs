using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public interface IAggregateManager<TAggregate, TAggregateId>
        where TAggregate: IAggregate<TAggregateId>
    {
        Task<TAggregate> LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken);

        Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken);
    }
}
