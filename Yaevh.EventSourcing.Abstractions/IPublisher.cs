using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing
{
    /// <summary>
    /// Informs the external component of an event
    /// </summary>
    public interface IPublisher
    {
        Task Publish<TAggregateId>(DomainEvent<TAggregateId> @event, CancellationToken cancellationToken);
    }
}
