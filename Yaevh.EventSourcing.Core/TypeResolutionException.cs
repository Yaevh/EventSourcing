namespace Yaevh.EventSourcing.Core;

// TODO: add test for the following exceptions
/// <summary>
/// Base class for all failures related to resolving an event or aggregate type
/// from its type identity (AssemblyQualifiedName, EventName/AggregateName, aliases, etc.).
/// </summary>
public abstract class TypeResolutionException : Exception
{
    protected TypeResolutionException(string message) : base(message) { }
    protected TypeResolutionException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotAnEventTypeException : TypeResolutionException
{
    public static readonly string DefaultMessage = $"The following types do not implement {nameof(IEventPayload)}: {{0}}. Check whether the types implement the interface and are accessible.";
    public NotAnEventTypeException(string message) : base(message) { }
    public NotAnEventTypeException(string message, Exception innerException) : base(message, innerException) { }
    public static NotAnEventTypeException ForType(IEnumerable<Type> types)
        => new NotAnEventTypeException(string.Format(DefaultMessage,string.Join("; ", types.Select(type => type.AssemblyQualifiedName))));
}

// ---------------------------------------------------------------------
// DOMAIN / EVENT CONTRACT FAILURES
// ---------------------------------------------------------------------

/// <summary>
/// Base class for failures caused by incorrect event naming, identity usage,
/// ambiguous matches, or contract changes. These indicate developer mistakes
/// in how event identities are used.
/// It's up to the developer to fix these issues in code, configuration, or migration.
/// </summary>
public abstract class EventResolutionException : TypeResolutionException
{
    protected EventResolutionException(string message) : base(message) { }
    protected EventResolutionException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when an event name (from [EventName] or similar identity)
/// does not correspond to any known event type.
/// </summary>
public class UnknownEventNameException : EventResolutionException
{
    public const string DefaultMessage = "Failed to resolve event type named {0}. Check whether the type name is correct and registered and whether the type is accessible.";
    public UnknownEventNameException(string message) : base(message) { }
    public UnknownEventNameException(string message, Exception innerException) : base(message, innerException) { }

    public static UnknownEventNameException ForTypeName(string typeName)
        => new UnknownEventNameException(string.Format(DefaultMessage, typeName));
}

/// <summary>
/// Thrown when an event name (from [EventName] or similar identity)
/// does not correspond to any known event type.
/// </summary>
public class UnknownEventTypeException : EventResolutionException
{
    public const string DefaultMessage = "Failed to resolve event type {0}. Check whether the type is registered correctly and whether the type is accessible.";
    public UnknownEventTypeException(string message) : base(message) { }
    public UnknownEventTypeException(string message, Exception innerException) : base(message, innerException) { }

    public static UnknownEventTypeException ForType(Type type)
        => new UnknownEventTypeException(string.Format(DefaultMessage, type.AssemblyQualifiedName));
}

/// <summary>
/// Thrown when an event name resolves to multiple event types,
/// indicating an ambiguous or conflicting identity.
/// </summary>
public class EventTypeAmbiguousException : EventResolutionException
{
    public const string DefaultMessage = "Event type {0} is ambiguous and maps to multiple event names. Check for duplicate event types in parameter list.";
    public EventTypeAmbiguousException(string message) : base(message) { }
    public EventTypeAmbiguousException(string message, Exception innerException) : base(message, innerException) { }
    public static EventTypeAmbiguousException ForType(Type type)
        => new EventTypeAmbiguousException(string.Format(DefaultMessage, type.AssemblyQualifiedName));
}

/// <summary>
/// Thrown when an event name resolves to multiple event types,
/// indicating an ambiguous or conflicting identity.
/// </summary>
public class EventNameAmbiguousException : EventResolutionException
{
    public const string DefaultMessage = "Event name '{0}' is ambiguous and maps to multiple event types. Check for duplicate [EventName] attributes or conflicting type names.";
    public EventNameAmbiguousException(string message) : base(message) { }
    public EventNameAmbiguousException(string message, Exception innerException) : base(message, innerException) { }
    public static EventNameAmbiguousException ForTypeName(string typeName)
        => new EventNameAmbiguousException(string.Format(DefaultMessage, typeName));
}

/// <summary>
/// Thrown when the developer uses the wrong identity mechanism.
/// Example: using AssemblyQualifiedName for an event that declares a custom [EventName].
/// </summary>
public class WrongEventIdentityException : EventResolutionException
{
    public const string DefaultMessage = "Attempted to use invalid identity mechanism, for example using AssemblyQualifiedName for an event decorated with EventNameAttribute.";
    public const string DefaultMessageWithType = "Attempted to use invalid identity mechanism for type {0}, for example using AssemblyQualifiedName for an event decorated with EventNameAttribute.";

    public WrongEventIdentityException() : this(DefaultMessage) { }
    public WrongEventIdentityException(string message) : base(message) { }
    public WrongEventIdentityException(string message, Exception innerException) : base(message, innerException) { }
    public static WrongEventIdentityException ForTypeName(string typeName)
        => new WrongEventIdentityException(string.Format(DefaultMessageWithType, typeName));
}
