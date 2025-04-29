using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Signifies an unknown event in the event stream; that is, an event that we don't know how to handle
    /// </summary>
    public class UnknownEventException : Exception
    {
        public UnknownEventException(Type eventType) : base($"Unknown event: {eventType.AssemblyQualifiedName}")
        {
        }
    }
}
