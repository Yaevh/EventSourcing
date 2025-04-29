using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.EFCore
{
    public record EventData<TAggregateId>(
        Guid EventId,
        DateTimeOffset DateTime,
        string EventName,
        TAggregateId AggregateId,
        string AggregateName,
        long EventIndex,
        string Payload)
        where TAggregateId : notnull;
}
