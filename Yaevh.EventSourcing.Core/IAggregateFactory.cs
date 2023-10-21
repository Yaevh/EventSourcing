using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public interface IAggregateFactory
    {
        TAggregate Create<TAggregate, TAggregateId>(TAggregateId aggregateId);
    }

    public class DefaultAggregateFactory : IAggregateFactory
    {

        public TAggregate Create<TAggregate, TAggregateId>(TAggregateId aggregateId)
        {
            var constructors = typeof(TAggregate).GetConstructors()
                .Where(FilterConstructor<TAggregateId>)
                .ToList();
            if (constructors.Count != 1)
                throw new ArgumentException($"{typeof(TAggregate).Name} should contain exactly one constructor with {typeof(TAggregateId).Name} as a single parameter");

            var constructor = constructors[0];

            return (TAggregate)constructor.Invoke(new object[] { aggregateId });
        }

        private bool FilterConstructor<TAggregateId>(ConstructorInfo constructor)
        {
            if (constructor.IsPublic == false)
                return false;

            var parameters = constructor.GetParameters();

            return parameters.Length == 1 && parameters[0].ParameterType == typeof(TAggregateId);
        }
    }
}
