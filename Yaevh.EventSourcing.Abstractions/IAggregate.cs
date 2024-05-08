using System;
using System.Collections.Generic;
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
        /// of the latest <see cref="DomainEvent{TAggregateId}"/> applied to this Aggregate
        /// </summary>
        long Version { get; }

        void Load(IEnumerable<DomainEvent<TAggregateId>> events);

        Task<IReadOnlyList<DomainEvent<TAggregateId>>> CommitAsync(IAggregateStore aggregateStore, CancellationToken cancellationToken);
    }
}
