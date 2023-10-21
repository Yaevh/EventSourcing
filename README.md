# Yaevh.EventSourcing
Just another library for event sourcing in .NET.

There are some Event Sourcing (ES) libraries on GitHub, but most of them fall into either of the two categories:
1. they're not libraries, but rather frameworks, and frameworks limit your flexibility - they force you to adapt a certain architecture and shape your project accordingly
2. if they're libraries, they tend to force you to adapt certain base classes for your domain objects; and I want to use POCOs as much as possible

This library aims at several goals:
1. To be simple and unopinionated
2. To be just a library, and not a framework; thus, to be opt-in, and not force you to use the whole framework to achieve your goals
3. Thus, to be simple to incorporate into your existing project as an opt-in feature
4. All the features other than core ES functionality should be opt-in and implemented as separate packages; those "other" features include: persistence, read models, snapshots, duplicate command detection, mediator issues etc.
5. To depend on some external libraries, but only those simple and non-polluting (as for now, it's only Ardalis.GuardClauses)


Right now, the library contains some basic ES functionalities, and basic persistence using SQLite.

It's still in a very basic state of development, so please expect it to be very unstable.


# Example usage

```c#
public class MyAggregate : Aggregate<MyAggregate>
// can also use IAggregate<MyAggregate>, but then you'll have to implement versioning etc. yourself
{
    // our basic data/state
    public string? CurrentValue { get; private set; }

    // the constructor; required to set the aggregate ID
    public MyAggregate(Guid aggregateId) : base(aggregateId) { }


    // the command we can issue on the aggregate; can be anything you want
    public void DoSomething(string value, DateTimeOffset now)
    {
        RaiseEvent(new MyEvent(value), now);
    }

    // how the aggregate should handle events
    protected override void Apply(IEvent aggregateEvent)
    {
        switch (aggregateEvent)
        {
            case MyEvent @event:
                CurrentValue = @event.Value;
                break;
            default:
                throw new UnknownEventException(aggregateEvent.GetType());
        }
    }

    // definitions of our events
    public record MyEvent(string Value) : IEvent;
}


// create the aggregate and issue some commands on it
var aggregate = new MyAggregate(Guid.NewGuid());
var now1 = DateTimeOffset.Now;
var now2 = now1 + TimeSpan.FromMinutes(1);
var now3 = now2 + TimeSpan.FromHours(24);
aggregate.DoSomething("one", now1);
aggregate.DoSomething("two", now2);
aggregate.DoSomething("three", now3);

// save the aggregate to the underlying AggregateStore
await AggregateManager.CommitAsync(aggregate, CancellationToken.None);

// restore the aggregate
var restoredAggregate = await AggregateManager.LoadAsync(aggregate.AggregateId, CancellationToken.None);

restoredAggregate.AggregateId.Should().Be(aggregate.AggregateId);
restoredAggregate.Version.Should().Be(3);
restoredAggregate.Value.Should().Be("three");
restoredAggregate.UncommittedEvents.Should().BeEmpty();
```