
using Microsoft.Extensions.DependencyInjection;

namespace Yaevh.EventSourcing.Example;

internal class Publisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public Publisher(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }


    public Task Publish<TAggegate, TAggregateId>(TAggegate aggegate, AggregateEvent<TAggregateId> @event, CancellationToken cancellationToken)
        where TAggegate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        var eventHandlerType = typeof(IAggregateEventHandler<,,>).MakeGenericType(typeof(TAggegate), typeof(TAggregateId), @event.Payload.GetType());

        try
        {
            var method = eventHandlerType.GetMethod("Handle")!;

            _ = Task.Run(async () => {
                // Resolve all registered handlers for the event type
                using var scope = _serviceScopeFactory.CreateScope();

                var handlers = (IEnumerable<object>)scope.ServiceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(eventHandlerType))!;

                var tasks = handlers.Select(handler => {
                    var method = handler.GetType().GetMethod("Handle")!;
                    return (Task)method.Invoke(handler, [aggegate, @event.Payload, cancellationToken])!;
                });

                await Task.WhenAll(tasks);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error handling event: {ex}");
        }

        return Task.CompletedTask;
    }
}
