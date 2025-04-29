using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Informs the external components of that an AggregateEvent took place.
    /// You may want to implement this interface to integrate with MassTransit or other messaging framework.
    /// </summary>
    public interface IPublisher
    {
        Task Publish<TAggregateId>(AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
            where TAggregateId : notnull;
    }
}
