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
    {
        private readonly IAggregateStore _store;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly IPublisher _publisher;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public AggregateManager(
            IAggregateStore store,
            IAggregateFactory aggregateFactory,
            IPublisher publisher,
            Microsoft.Extensions.Logging.ILogger<AggregateManager<TAggregate, TAggregateId>> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _aggregateFactory = aggregateFactory ?? throw new ArgumentNullException(nameof(aggregateFactory));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TAggregate> LoadAsync(TAggregateId aggregateId, CancellationToken cancellationToken)
        {
            var aggregate = _aggregateFactory.Create<TAggregate, TAggregateId>(aggregateId);
            var events = await _store.LoadAsync(aggregateId, cancellationToken);
            aggregate.Load(events);
            return aggregate;
        }

        public async Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken)
        {
            var events = await aggregate.CommitAsync(_store, cancellationToken);

            foreach (var @event in events)
            {
                await _publisher.Publish(@event, cancellationToken);
            }
        }
    }
}
