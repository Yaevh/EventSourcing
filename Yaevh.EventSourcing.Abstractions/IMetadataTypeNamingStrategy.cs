namespace Yaevh.EventSourcing;

public interface IMetadataTypeNamingStrategy
{
    string ToUniqueName(Type metadataType);
    Type FromUniqueName(string metadataTypeName);
}

// TODO move this to Core project and implement it properly, this is just a placeholder for now
// TODO add tests
public class DefaultMetadataTypeNamingStrategy : IMetadataTypeNamingStrategy
{
    public string ToUniqueName(Type metadataType)
    {
        return metadataType.AssemblyQualifiedName!;
    }

    public Type FromUniqueName(string metadataTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metadataTypeName);
        // TODO add type cache to avoid repeated calls to Type.GetType()
        return Type.GetType(metadataTypeName, throwOnError: true)
            ?? throw new TypeLoadException($"Type '{metadataTypeName}' could not be found.");
    }
}