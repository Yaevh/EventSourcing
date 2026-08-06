using System.Collections.Concurrent;
using System.Reflection;

namespace Yaevh.EventSourcing.Core;

// TODO add tests
public class DefaultEventTypeNamingStrategy : IEventTypeNamingStrategy
{
    private static readonly Type EventNameAttributeType = typeof(EventNameAttribute);

    private static readonly ConcurrentDictionary<Type, string> _typeToNameCache = new();
    private static readonly ConcurrentDictionary<string, Type> _nameToTypeCache = new(StringComparer.Ordinal);

    static DefaultEventTypeNamingStrategy()
    {
        BuildEventNameAttributesCache();
    }


    public string ToUniqueName(Type eventType)
    {
        return _typeToNameCache.GetOrAdd(eventType, ToUniqueNameImpl);
    }

    private string ToUniqueNameImpl(Type eventType)
    {
        return eventType.GetCustomAttributes<EventNameAttribute>(inherit: false)
            .SingleOrDefault()?.Value
            ??
            eventType.AssemblyQualifiedName!;
    }

    public Type FromUniqueName(string eventTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventTypeName);
        return _nameToTypeCache.GetOrAdd(eventTypeName, FromUniqueNameImpl);
    }

    private Type FromUniqueNameImpl(string eventTypeName)
    {
        // the cache is pre-populated with all types that have the EventNameAttribute,
        // so if the name is found in the cache, we have to return the type directly
        return Type.GetType(eventTypeName, throwOnError: true)!;
    }

    private static void BuildEventNameAttributesCache()
    {
        var eventNameAttributes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => new {
                Type = t,
                Attribute = t.CustomAttributes
                    .Where(x => x.AttributeType == EventNameAttributeType)
                    .SingleOrDefault()
            })
            .Where(x => x.Attribute != null)
            .Select(x => new {
                Type = x.Type,
                EventName = x.Attribute!.ConstructorArguments[0].Value as string
            });

        foreach (var item in eventNameAttributes)
            _nameToTypeCache.TryAdd(item.EventName!, item.Type);
    }
}