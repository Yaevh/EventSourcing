using Microsoft.Extensions.DependencyInjection;

namespace Yaevh.EventSourcing.Example;

internal class Publisher : IPublisher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public Publisher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }


    public async Task Publish<TAggregate, TAggregateId>(TAggregate aggregate, AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
        where TAggregate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        using var scope = _serviceScopeFactory.CreateScope();

        await EventDispatcher.DispatchEvent(aggregate, @event, scope.ServiceProvider, cancellationToken);
    }
}
