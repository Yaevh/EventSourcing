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
