using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public class AggregateManager<TAggregate, TAggregateId>
        : IAggregateManager<TAggregate, TAggregateId>
        where TAggregate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        private readonly IEventStore<TAggregateId> _store;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly IPublisher _publisher;
        private readonly ILogger _logger;

        public AggregateManager(
            IEventStore<TAggregateId> store,
            IAggregateFactory aggregateFactory,
            IPublisher publisher,
            ILogger<AggregateManager<TAggregate, TAggregateId>> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _aggregateFactory = aggregateFactory ?? throw new ArgumentNullException(nameof(aggregateFactory));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TAggregate> LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Trying to load aggregate with AggregateId = {AggregateId}", aggregateId);
            var aggregate = _aggregateFactory.Create<TAggregate, TAggregateId>(aggregateId);
            var events = await _store.LoadAsync(aggregateId, cancellationToken);
            _logger.LogDebug("Found {EventCount} events for aggregate {AggregateType} with ID {AggregateId}, now loading", events.Count(), aggregate.GetType().FullName, aggregateId);
            aggregate.Load(events);
            _logger.LogDebug("Loaded {EventCount} events for aggregate {AggregateType} with ID {AggregateId}", events.Count(), aggregate.GetType().FullName, aggregateId);
            return aggregate;
        }

        public async Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken)
        {
            var events = await aggregate.CommitAsync(_store, cancellationToken);

            foreach (var @event in events)
            {
                await _publisher.Publish(aggregate, @event, cancellationToken);
            }
        }
    }
}
