using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Persistence;

public interface IAggregateIdSerializer { }

public interface IAggregateIdSerializer<TAggregateId> : IAggregateIdSerializer
{
    string Serialize(TAggregateId aggregateId);
    TAggregateId Deserialize(string serializedValue);
}
