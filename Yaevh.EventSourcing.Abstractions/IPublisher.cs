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
        Task Publish<TAggregate, TAggregateId>(TAggregate aggregate, AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
            where TAggregate : IAggregate<TAggregateId>
            where TAggregateId : notnull;
    }


    /// <summary>
    /// An implementation of <see cref="IPublisher"/> that does nothing.
    /// </summary>
    public class NullPublisher : IPublisher
    {
        public Task Publish<TAggregate, TAggregateId>(TAggregate aggregate, AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
            where TAggregate : IAggregate<TAggregateId>
            where TAggregateId : notnull
        {
            return Task.CompletedTask;
        }
    }
}
