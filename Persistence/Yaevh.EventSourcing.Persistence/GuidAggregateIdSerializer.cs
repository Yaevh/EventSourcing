using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Persistence;

public class GuidAggregateIdSerializer : IAggregateIdSerializer<Guid>
{
    public Guid Deserialize(string serializedValue)
    {
        return Guid.Parse(serializedValue);
    }

    public string Serialize(Guid aggregateId)
    {
        return aggregateId.ToString().ToUpperInvariant();
    }
}
