using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    /// <summary>
    /// Creates new instances of aggregates by invoking the constructor taking a single AggregateId as a parameter
    /// </summary>
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public TAggregate Create<TAggregate, TAggregateId>(TAggregateId aggregateId)
            where TAggregate : IAggregate<TAggregateId>
            where TAggregateId : notnull
        {
            var constructors = typeof(TAggregate).GetConstructors()
                .Where(FilterConstructor<TAggregateId>)
                .ToList();
            if (constructors.Count != 1)
                throw new ArgumentException($"The aggregate \"{typeof(TAggregate).Name}\" should contain exactly one constructor with {typeof(TAggregateId).Name} as a single parameter");

            var constructor = constructors[0];

            return (TAggregate)constructor.Invoke(new object[] { aggregateId });
        }

        private static bool FilterConstructor<TAggregateId>(ConstructorInfo constructor)
        {
            if (constructor.IsPublic == false)
                return false;

            var parameters = constructor.GetParameters();

            return parameters.Length == 1 && parameters[0].ParameterType == typeof(TAggregateId);
        }
    }
}
