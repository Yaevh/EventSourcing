namespace Yaevh.EventSourcing.Example.Model.Events;

public record AccountClosed : IEventPayload
{
    public required DateTimeOffset ClosedAt { get; init; }
}
