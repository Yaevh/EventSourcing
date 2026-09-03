namespace Yaevh.EventSourcing;

public interface IEventTypeNamingStrategy
{
    string ToUniqueName(Type eventType);
    Type FromUniqueName(string eventTypeName);
}
