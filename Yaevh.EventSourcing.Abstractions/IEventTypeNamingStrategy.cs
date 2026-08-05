namespace Yaevh.EventSourcing;

public interface IEventTypeNamingStrategy
{
    string ToUniqueName(Type eventType);
    Type FromUniqueName(string eventTypeName);
}

// TODO move this to Core project and implement it properly, this is just a placeholder for now
// TODO add tests
public class DefaultEventTypeNamingStrategy : IEventTypeNamingStrategy
{
    public string ToUniqueName(Type eventType)
    {
        return eventType.AssemblyQualifiedName!;
    }

    public Type FromUniqueName(string eventTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventTypeName);
        // TODO add type cache to avoid repeated calls to Type.GetType()
        return Type.GetType(eventTypeName, throwOnError: true)
            ?? throw new TypeLoadException($"Type '{eventTypeName}' could not be found.");
    }
}