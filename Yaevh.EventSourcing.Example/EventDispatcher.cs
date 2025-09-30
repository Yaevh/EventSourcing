namespace Yaevh.EventSourcing.Example;

public static class EventDispatcher
{
    /// <summary>
    /// Dispatches a collection of committed aggregate events to their respective handlers.
    /// </summary>
    /// <remarks>This method resolves all event handlers for each event type using the provided <paramref
    /// name="serviceProvider"/>  and invokes their <c>Handle</c> method asynchronously. If no handlers are registered
    /// for a specific event type,  the event is ignored. The method processes events sequentially and invokes all
    /// handlers for each event.</remarks>
    /// <typeparam name="TAggegate">The type of the aggregate associated with the events.</typeparam>
    /// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
    /// <param name="aggegate">The aggregate instance associated with the events.</param>
    /// <param name="events">The collection of committed events to be dispatched.</param>
    /// <param name="serviceProvider">The service provider used to resolve event handlers.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task DispatchEvents<TAggegate, TAggregateId>(
        TAggegate aggegate,
        IEnumerable<AggregateEvent<TAggregateId>> events,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TAggegate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        foreach (var @event in events)
        {
            var eventHandlerType = typeof(IAggregateEventHandler<,,>).MakeGenericType(typeof(TAggegate), typeof(TAggregateId), @event.Payload.GetType());
            var method = eventHandlerType.GetMethod("Handle")!;

            // Resolve all registered handlers for the event type
            var handlers = (IEnumerable<object>)serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(eventHandlerType))!;

            foreach (var handler in handlers)
                await (Task)method.Invoke(handler, [aggegate, @event.Payload, cancellationToken])!;
        }
    }

    /// <summary>
    /// Dispatches a single domain event for the specified aggregate.
    /// </summary>
    /// <remarks>This method dispatches the specified event to the appropriate handlers. It ensures that the
    /// event is processed in the context of the provided aggregate. The method is asynchronous and will complete once
    /// the event has been dispatched to all relevant handlers.</remarks>
    /// <typeparam name="TAggegate">The type of the aggregate associated with the event.</typeparam>
    /// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
    /// <param name="aggegate">The aggregate instance that the event is associated with. Cannot be <see langword="null"/>.</param>
    /// <param name="event">The domain event to dispatch. Cannot be <see langword="null"/>.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies for event handling. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task DispatchEvent<TAggegate, TAggregateId>(
        TAggegate aggegate,
        AggregateEvent<TAggregateId> @event,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TAggegate : IAggregate<TAggregateId>
        where TAggregateId : notnull
    {
        await DispatchEvents(aggegate, new[] { @event }, serviceProvider, cancellationToken);
    }
}
