using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public interface IAggregateStore
    {
        Task<IEnumerable<DomainEvent<TAggregateId>>> LoadAsync<TAggregateId>(TAggregateId aggregateId, CancellationToken cancellationToken);
        Task StoreAsync<TAggregate, TAggregateId>(TAggregate aggregate, IReadOnlyList<DomainEvent<TAggregateId>> events, CancellationToken cancellationToken)
            where TAggregate : IAggregate<TAggregateId>;
    }
}
