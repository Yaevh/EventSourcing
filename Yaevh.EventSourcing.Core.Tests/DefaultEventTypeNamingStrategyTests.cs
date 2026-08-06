namespace Yaevh.EventSourcing.Core.Tests;

public class DefaultEventTypeNamingStrategyTests
{
    [Fact]
    public void DefaultEventTypeNamingStrategy_ShouldThrow_NotAnEventTypeException_WhenNonEventTypesRegistered()
    {
        Action act = () => new DefaultEventTypeNamingStrategy(
            [typeof(SimpleEvent), typeof(string)]); // string is not an event type
        act.Should()
            .Throw<NotAnEventTypeException>()
            .WithMessage($"The following types do not implement {nameof(IEventPayload)}: {typeof(string).AssemblyQualifiedName}. Check whether the types implement the interface and are accessible.");
    }

    [Fact]
    public void ToUniqueName_Uses_EventNameAttribute_WhenPresent()
    {
        // Arrange
        var eventName = "Event with name";
        var eventType = typeof(EventWithName); // this has an EventNameAttribute with Name = "Event with name"
        var strategy = new DefaultEventTypeNamingStrategy([eventType]);
        
        // Act
        var uniqueName = strategy.ToUniqueName(eventType);

        // Assert
        uniqueName.Should().Be(eventName);
    }

    [Fact]
    public void ToUniqueName_FallsBack_To_AssemblyQualifiedName_WhenNoAttribute()
    {
        // Arrange
        var eventType = typeof(SimpleEvent); // no EventNameAttribute on SimpleEvent
        var strategy = new DefaultEventTypeNamingStrategy([eventType]);

        // Act
        var uniqueName = strategy.ToUniqueName(eventType);

        // Assert
        uniqueName.Should().Be(eventType.AssemblyQualifiedName);
    }

    [Fact]
    public void FromUniqueName_Resolves_AttributeName()
    {
        // Arrange
        var eventType = typeof(EventWithName);
        var attributeName = "Event with name"; 
        var strategy = new DefaultEventTypeNamingStrategy([typeof(EventWithName)]);
        
        // Act
        var resolvedFromAttribute = strategy.FromUniqueName(attributeName);

        // Assert
        resolvedFromAttribute.Should().Be(eventType);
    }


    // -------------------------------------------------------------
    // UnknownEventTypeException
    // -------------------------------------------------------------
    [Fact]
    public void FromUniqueName_ShouldThrow_UnknownEventNameException_WhenNameNotRegistered()
    {
        var strategy = new DefaultEventTypeNamingStrategy([]);
        Action act = () => strategy.FromUniqueName("NonExistingEvent");

        act.Should()
            .Throw<UnknownEventNameException>()
            .WithMessage("Failed to resolve event type named*NonExistingEvent*. Check whether the type name is correct and registered and whether the type is accessible*");
    }

    // -------------------------------------------------------------
    // UnknownEventTypeException
    // -------------------------------------------------------------
    [Fact]
    public void ToUniqueName_ShouldThrow_UnknownEventTypeException_WhenNameNotRegistered()
    {
        var strategy = new DefaultEventTypeNamingStrategy([]);
        Action act = () => strategy.ToUniqueName(typeof(SimpleEvent));

        act.Should()
            .Throw<UnknownEventTypeException>()
            .WithMessage($"Failed to resolve event type {typeof(SimpleEvent).AssemblyQualifiedName}. Check whether the type is registered correctly*");
    }

    // -------------------------------------------------------------
    // EventNameAmbiguousException
    // -------------------------------------------------------------
    [Fact]
    public void DefaultEventTypeNamingStrategy_ShouldThrow_EventNameAmbiguousException_WhenMultipleTypesRegisteredWithTheSameName()
    {
        Action act = () => new DefaultEventTypeNamingStrategy(
            [typeof(EventWithDuplicateName1), typeof(EventWithDuplicateName2)]);

        act.Should()
            .Throw<EventNameAmbiguousException>()
            .WithMessage("Event name 'Event with duplicate name' is ambiguous and maps to multiple event types. Check for duplicate [EventName] attributes or conflicting type names.");
    }

    // -------------------------------------------------------------
    // EventNameAmbiguousException
    // -------------------------------------------------------------
    [Fact]
    public void DefaultEventTypeNamingStrategy_ShouldThrow_EventTypeAmbiguousException_WhenSingleTypeRegisteredMultipleTimes()
    {
        Action act = () => new DefaultEventTypeNamingStrategy(
            [typeof(EventWithName), typeof(EventWithName)]);

        act.Should()
            .Throw<EventTypeAmbiguousException>()
            .WithMessage($"*Event type {typeof(EventWithName).AssemblyQualifiedName} is ambiguous and maps to multiple event names*");
    }

    // -------------------------------------------------------------
    // WrongEventIdentityException
    // -------------------------------------------------------------
    [Fact]
    public void FromUniqueName_ShouldThrow_UnknownEventNameException_WhenWrongIdentityUsed()
    {
        var strategy = new DefaultEventTypeNamingStrategy([typeof(EventWithName)]);

        Action act = () => strategy.FromUniqueName(typeof(EventWithName).AssemblyQualifiedName!);

        act.Should()
            .Throw<UnknownEventNameException>()
            .Which.Message.Should().Contain(typeof(EventWithName).AssemblyQualifiedName);
    }

    #region test event definitions

    internal record class SimpleEvent(string Value) : IEventPayload;

    [EventName("Event with name")]
    internal record EventWithName(string Value) : IEventPayload;

    [EventName("Event with duplicate name")]
    internal record EventWithDuplicateName1(string Value) : IEventPayload;

    [EventName("Event with duplicate name")]
    internal record EventWithDuplicateName2(string Value) : IEventPayload;

    #endregion
}
