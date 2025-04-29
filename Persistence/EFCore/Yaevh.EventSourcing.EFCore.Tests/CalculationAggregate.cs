using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.EFCore.Tests;

public class CalculationAggregate : Aggregate<CalculationAggregate>
{
    public CalculationAggregate(Guid id) : base(id) { }
    public CalculationAggregate(decimal initialValue, Guid id) : base(id)
    {
        Value = initialValue;
    }

    public decimal Value { get; private set; } = 0;

    #region public methods
    public void Add(decimal value) => RaiseEvent(new AdditionEvent(value), DateTimeOffset.Now);
    public void Subtract(decimal value) => RaiseEvent(new SubtractionEvent(value), DateTimeOffset.Now);
    public void Multiply(decimal value) => RaiseEvent(new MultiplicationEvent(value), DateTimeOffset.Now);
    public void Divide(decimal value) => RaiseEvent(new DivisionEvent(value), DateTimeOffset.Now);
    #endregion

    protected override void Apply(IEventPayload aggregateEvent)
    {
        Value = aggregateEvent switch {
            AdditionEvent add => Value + add.Value,
            SubtractionEvent sub => Value - sub.Value,
            MultiplicationEvent mul => Value * mul.Value,
            DivisionEvent div => Value / div.Value,
            _ => throw new UnknownEventException(aggregateEvent.GetType())
        };
    }

    #region events
    public record AdditionEvent(decimal Value) : IEventPayload;
    public record SubtractionEvent(decimal Value) : IEventPayload;
    public record MultiplicationEvent(decimal Value) : IEventPayload;
    public record DivisionEvent(decimal Value) : IEventPayload;
    #endregion
}
