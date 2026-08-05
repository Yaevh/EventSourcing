namespace Yaevh.EventSourcing;

public interface IAggregateTypeNamingStrategy
{
    string ToUniqueName(Type aggregateType);
    Type FromUniqueName(string aggregateTypeName);
}

// TODO move this to Core project and implement it properly, this is just a placeholder for now
// TODO add tests
public class DefaultAggregateTypeNamingStrategy : IAggregateTypeNamingStrategy
{
    public string ToUniqueName(Type aggregateType)
    {
        return aggregateType.AssemblyQualifiedName!;
    }

    public Type FromUniqueName(string aggregateTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateTypeName);
        // TODO add type cache to avoid repeated calls to Type.GetType()
        return Type.GetType(aggregateTypeName, throwOnError: true)
            ?? throw new TypeLoadException($"Type '{aggregateTypeName}' could not be found.");
    }
}