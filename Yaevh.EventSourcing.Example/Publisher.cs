
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

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

                foreach (var handler in handlers)
                {
                    var task = (Task)method.Invoke(handler, [aggegate, @event.Payload, cancellationToken])!;
                    await task.ContinueWith(t => {
                        if (t.IsFaulted)
                        {
                            // Log the exception or handle it as needed
                            Console.WriteLine($"Error in event handler: {t.Exception}");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
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
