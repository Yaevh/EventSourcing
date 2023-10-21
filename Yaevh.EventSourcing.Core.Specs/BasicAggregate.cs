using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core.Specs
{
    /// <summary>
    /// A simple Aggregate that counts the times CountUpEvent was raised
    /// </summary>
    public class BasicAggregate : Aggregate<BasicAggregate>
    {
        public string? Value { get; private set; }

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
                    Value = @event.Value;
                    break;
                default:
                    throw new UnknownEventException(aggregateEvent.GetType());
            }
        }


        internal record BasicEvent(string Value) : IEvent;
    }
}
