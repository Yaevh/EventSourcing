using System.Collections.Concurrent;
using System.Reflection;

namespace Yaevh.EventSourcing.Core;

public class DefaultEventTypeNamingStrategy : IEventTypeNamingStrategy
{
    private static readonly Type EventNameAttributeType = typeof(EventNameAttribute);
    private static readonly Type IEventPayloadType = typeof(IEventPayload);

    private readonly Dictionary<Type, string> _typeToNameCache = new();
    private readonly Dictionary<string, Type> _nameToTypeCache = new(StringComparer.Ordinal);
    

    public DefaultEventTypeNamingStrategy(IEnumerable<Type> knownEventTypes)
    {
        BuildEventNameAttributesCache(knownEventTypes);
    }

    public string ToUniqueName(Type eventType)
    {
        if (_typeToNameCache.TryGetValue(eventType, out var name))
            return name;
        else
            throw UnknownEventTypeException.ForType(eventType);
    }

    public Type FromUniqueName(string eventTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventTypeName);
        if (_nameToTypeCache.TryGetValue(eventTypeName, out var type))
            return type;
        else
            throw UnknownEventNameException.ForTypeName(eventTypeName);
    }

    private void BuildEventNameAttributesCache(IEnumerable<Type> eventTypes)
    {
        if (eventTypes.Any(t => t.IsAssignableTo(IEventPayloadType) == false))
            throw NotAnEventTypeException.ForType(eventTypes.Where(t => t.IsAssignableTo(IEventPayloadType) == false));

        var eventNameAttributes = eventTypes
            .Select(t => new {
                Type = t,
                Attribute = t.CustomAttributes
                    .SingleOrDefault(x => x.AttributeType == EventNameAttributeType)
            })
            .Select(x => new {
                Type = x.Type,
                EventName = x.Attribute?.ConstructorArguments[0].Value as string ?? x.Type.AssemblyQualifiedName
            });

        foreach (var item in eventNameAttributes)
        {
            AddToCache(item.Type, item.EventName!);
        }
    }

    private void AddToCache(Type eventType, string eventName)
    {
        if (_typeToNameCache.TryAdd(eventType, eventName) == false)
            throw EventTypeAmbiguousException.ForType(eventType);
        if (_nameToTypeCache.TryAdd(eventName, eventType) == false)
            throw EventNameAmbiguousException.ForTypeName(eventName);
    }
}