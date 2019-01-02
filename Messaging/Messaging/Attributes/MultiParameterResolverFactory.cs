using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAxonFramework.Messaging.Attributes
{
    public class MultiParameterResolverFactory : IParameterResolverFactory
    {
        private IParameterResolverFactory[] _factories;

        public MultiParameterResolverFactory(IParameterResolverFactory[] factories)
        {
            _factories = factories.ToArray();
        }

        public static MultiParameterResolverFactory Ordered(params IParameterResolverFactory[] factories)
        {
            return Ordered(factories.ToList());
        }
        public static MultiParameterResolverFactory Ordered(List<IParameterResolverFactory> factories)
        {
            return new MultiParameterResolverFactory(Flatten(factories));
        }

        private static IParameterResolverFactory[] Flatten(IEnumerable<IParameterResolverFactory> factories)
        {
            return factories.SelectMany(x =>
            {
                if (x is MultiParameterResolverFactory nested)
                    return nested._factories;
                return new[] {x};
            }).ToArray();
        }

        public IEnumerable<IParameterResolverFactory> Delegates => _factories.AsEnumerable();

        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            foreach (IParameterResolverFactory factory in _factories) {
                IParameterResolver resolver = factory.CreateInstance(executable, parameters, parameterIndex);
                if (resolver != null) {
                    return resolver;
                }
            }
            return null; 
        }
    }
}