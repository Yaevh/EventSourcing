namespace Yaevh.EventSourcing.Example.Model.Events;

public record MoneyDeposited : IEventPayload
{
    public required decimal Amount { get; init; }
    public required Currency Currency { get; init; }
}
