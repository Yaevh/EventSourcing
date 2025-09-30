using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core.Tests
{
    /// <summary>
    /// A simple Aggregate that counts the times CountUpEvent was raised
    /// </summary>
    internal class BasicAggregate : Aggregate<BasicAggregate>
    {
        public string? Value { get; private set; }

        public BasicAggregate(Guid aggregateId) : base(aggregateId) { }


        public void DoSomething(string value, DateTimeOffset now)
        {
            RaiseEvent(new BasicEvent(value), now);
        }

        protected override void Apply(AggregateEvent<Guid> aggregateEvent)
        {
            switch (aggregateEvent.Payload)
            {
                case BasicEvent @event:
                    Value = @event.Value;
                    break;
                default:
                    throw new UnknownEventException(aggregateEvent.GetType());
            }
        }


        internal record BasicEvent(string Value) : IEventPayload;
    }
}
