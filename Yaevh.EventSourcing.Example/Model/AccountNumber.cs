using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.Example.Model;

public record AccountNumber
{
    public string Value { get; }
    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));
        if (!value.All(c => char.IsLetterOrDigit(c) || c == '-'))
            throw new ArgumentException("Account number can only contain letters, digits, and hyphens", nameof(value));
        Value = value;
    }
    public override string ToString() => Value;


    public class AggregateIdSerializer : IAggregateIdSerializer<AccountNumber>
    {
        public string Serialize(AccountNumber aggregateId) => aggregateId.Value;
        public AccountNumber Deserialize(string serialized) => new AccountNumber(serialized);
    }
}
