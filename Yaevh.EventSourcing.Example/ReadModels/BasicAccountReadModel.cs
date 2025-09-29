using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.ReadModels;

public class BasicAccountReadModel
{
    public AccountNumber AccountNumber { get; init; }
    public string OwnerName { get; init; }
    public decimal Balance { get; set; }
    public Currency Currency { get; init; }
    public bool IsClosed { get; set; }
}
