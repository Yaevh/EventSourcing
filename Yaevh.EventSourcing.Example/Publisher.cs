
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

namespace Yaevh.EventSourcing.Example;

internal class Publisher : IPublisher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public Publisher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }


    public Task Publish<TAggegate, TAggregateId>(TAggegate aggegate, AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
        where TAggegate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        _ = Task.Run(async () => {
            using var scope = _serviceScopeFactory.CreateScope();

            await EventDispatcher.DispatchEvent(aggegate, @event, scope.ServiceProvider, cancellationToken);
        }, cancellationToken);

        return Task.CompletedTask;
    }
}
