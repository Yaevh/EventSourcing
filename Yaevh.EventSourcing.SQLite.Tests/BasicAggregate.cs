using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.SQLite.Tests
{
    /// <summary>
    /// A simple Aggregate that counts the times CountUpEvent was raised
    /// </summary>
    public class BasicAggregate : Aggregate<BasicAggregate>
    {
        public string? CurrentValue { get; private set; }

        public BasicAggregate(Guid aggregateId) : base(aggregateId) { }


        public void DoSomething(string value, DateTimeOffset now)
        {
            RaiseEvent(new BasicEvent(value), now);
        }

        protected override void Apply(IEvent aggregateEvent)
        {
            switch (aggregateEvent)
            {
                case BasicEvent @event:
                    CurrentValue = @event.Value;
                    break;
                default:
                    throw new UnknownEventException(aggregateEvent.GetType());
            }
        }


        public record BasicEvent(string Value) : IEvent;
    }
}
