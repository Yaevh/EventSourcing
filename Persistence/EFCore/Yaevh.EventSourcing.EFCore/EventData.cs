namespace Yaevh.EventSourcing.EFCore
{
    public record EventData<TAggregateId>(
        string AggregateType,
        TAggregateId AggregateId,
        string EventType,
        long EventId,
        long EventIndex,
        DateTimeOffset DateTime,
        string Payload,
        string? MetadataType,
        string? Metadata)
        where TAggregateId : notnull;
}
