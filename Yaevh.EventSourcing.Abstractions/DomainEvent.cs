using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    public record DomainEvent<TAggregateId>(
        IEventPayload Payload,
        IEventMetadata<TAggregateId> Metadata
    );
}
