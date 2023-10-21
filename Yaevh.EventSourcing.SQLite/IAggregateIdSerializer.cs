using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.SQLite
{
    public interface IAggregateIdSerializer { }

    public interface IAggregateIdSerializer<TAggregateId> : IAggregateIdSerializer
    {
        string Serialize(TAggregateId aggregateId);
        TAggregateId Deserialize(string serializedValue);
    }

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
}
