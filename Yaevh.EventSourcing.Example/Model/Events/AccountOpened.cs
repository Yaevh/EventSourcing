namespace Yaevh.EventSourcing.Example.Model.Events;

public record AccountOpened : IEventPayload
{
    public required DateTimeOffset OpenedAt { get; init; }
    public required AccountNumber AccountNumber { get; init; }
    public required string OwnerName { get; init; } = string.Empty;
    public required Currency Currency { get; init; }
}
