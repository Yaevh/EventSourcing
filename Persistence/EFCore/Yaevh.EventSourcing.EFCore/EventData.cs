namespace Yaevh.EventSourcing.EFCore
{
    public record EventData<TAggregateId>(
        string AggregateName,
        TAggregateId AggregateId,
        string EventName,
        long EventId,
        long EventIndex,
        DateTimeOffset DateTime,
        string Payload,
        string? MetadataType,
        string? Metadata)
        where TAggregateId : notnull;
}
