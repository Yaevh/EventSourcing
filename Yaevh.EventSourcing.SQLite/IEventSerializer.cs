using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.SQLite
{
    public interface IEventSerializer
    {
        string Serialize<T>(T value);
        T? Deserialize<T>(string value);
        object? Deserialize(string value, Type type);
    }

    public class SystemTextJsonSerializer : IEventSerializer
    {
        private readonly System.Text.Json.JsonSerializerOptions? _serializerOptions;
        public SystemTextJsonSerializer(System.Text.Json.JsonSerializerOptions? serializerOptions = null)
        {
            _serializerOptions = serializerOptions;
        }

        public T? Deserialize<T>(string value) => System.Text.Json.JsonSerializer.Deserialize<T>(value, _serializerOptions);

        public object? Deserialize(string value, Type type) => System.Text.Json.JsonSerializer.Deserialize(value, type, _serializerOptions);

        public string Serialize<T>(T value)
        {
            // by default, System.Text.Json serializes only properties declared on the base type.
            // this is a workaround to force it to serialize all properties, including derived ones
            // this can be assumed to be safe, since all serialized objects would be declared by the developers
            var valueAsObject = value as object;
            return System.Text.Json.JsonSerializer.Serialize(valueAsObject, _serializerOptions);
        }
    }
}
