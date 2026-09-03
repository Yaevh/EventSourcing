namespace Yaevh.EventSourcing;

/// <summary>
/// Signifies an unknown event in the event stream; that is, an event that we don't know how to handle
/// </summary>
public class UnsupportedEventException : Exception
{
    public UnsupportedEventException(Type eventType) : base($"Unknown event: {eventType.AssemblyQualifiedName}")
    {
    }
}
