using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Persistence;

public class SystemTextJsonEventSerializer(System.Text.Json.JsonSerializerOptions? serializerOptions = null)
    : IEventSerializer
{
    public T? Deserialize<T>(string value) => (T?)Deserialize(value, typeof(T));

    public object? Deserialize(string value, Type type)
        => System.Text.Json.JsonSerializer.Deserialize(value, type, serializerOptions);

    public string Serialize<T>(T value)
    {
        // By default, System.Text.Json serializes only properties declared on the base type.
        // This is a workaround to force it to serialize all properties, including derived ones.
        // This can be assumed to be safe, since all serialized objects are declared in the code
        // and don't come from external sources
        var valueAsObject = value as object;
        return System.Text.Json.JsonSerializer.Serialize(valueAsObject, serializerOptions);
    }
}
