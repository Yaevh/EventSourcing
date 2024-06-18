using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Creates new instances of aggregates
    /// </summary>
    public interface IAggregateFactory
    {
        TAggregate Create<TAggregate, TAggregateId>(TAggregateId aggregateId)
            where TAggregate : IAggregate<TAggregateId>
            where TAggregateId : notnull;
    }
}
