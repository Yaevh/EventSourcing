using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Persistence
{
    public interface IEventSerializer
    {
        string Serialize<T>(T value);
        T? Deserialize<T>(string value);
        object? Deserialize(string value, Type type);
    }
}
