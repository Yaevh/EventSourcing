namespace Yaevh.EventSourcing.Example.Model.Events;

public record MoneyWithdrawn : IEventPayload
{
    public required decimal Amount { get; init; }
    public required Currency Currency { get; init; }
}
