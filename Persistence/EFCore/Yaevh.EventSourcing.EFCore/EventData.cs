using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.EFCore
{
    public record EventData<TAggregateId>(
        string AggregateName,
        TAggregateId AggregateId,
        string EventName,
        Guid EventId,
        long EventIndex,
        DateTimeOffset DateTime,
        string Payload,
        string? MetadataType,
        string? Metadata)
        where TAggregateId : notnull;
}
