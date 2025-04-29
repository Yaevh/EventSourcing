using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public abstract class Aggregate<TAggregate> : Aggregate<TAggregate, Guid>
        where TAggregate: Aggregate<TAggregate>
    {
        protected Aggregate(Guid aggregateId) : base(aggregateId) { }
    }
}
