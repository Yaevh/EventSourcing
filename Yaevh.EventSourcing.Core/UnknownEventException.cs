using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public class UnknownEventException : ApplicationException
    {
        public UnknownEventException(Type eventType) : base($"Unknown event: {eventType.AssemblyQualifiedName}")
        {
        }
    }
}
