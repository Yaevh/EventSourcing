namespace Yaevh.EventSourcing.Core;

// TODO add tests for this attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class EventNameAttribute : Attribute
{
    public EventNameAttribute(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }
}
