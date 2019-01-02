using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAxonFramework.Messaging.Attributes
{
    public class MultiHandlerEnhancerDefinition : IHandlerEnhancerDefinition
    {
        private readonly IHandlerEnhancerDefinition[] _enhancers;

        public MultiHandlerEnhancerDefinition(params IHandlerEnhancerDefinition[] delegates)
        {
            _enhancers = delegates.ToArray();
        }
        public MultiHandlerEnhancerDefinition(List<IHandlerEnhancerDefinition> delegates)
        {
            _enhancers = delegates.ToArray();
        }

        public static MultiHandlerEnhancerDefinition Ordered(params IHandlerEnhancerDefinition[] delegates)
        {
            return Ordered(delegates.ToList());
        }
        public static MultiHandlerEnhancerDefinition Ordered(List<IHandlerEnhancerDefinition> delegates)
        {
            return new MultiHandlerEnhancerDefinition(Flatten(delegates));
        }

        private static IHandlerEnhancerDefinition[] Flatten(IEnumerable<IHandlerEnhancerDefinition> delegates)
        {
            return delegates.SelectMany(x =>
            {
                if (x is MultiHandlerEnhancerDefinition nested)
                    return nested.Delegates;
                return new[] {x};
            }).ToArray();
        }

        public IEnumerable<IHandlerEnhancerDefinition> Delegates => _enhancers.AsEnumerable();



//        public IMessageHandlingMember<T> WrapHandler<T>(IMessageHandlingMember<T> original) => (IMessageHandlingMember<T>) WrapHandler(typeof(T), original);

        public IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original)
        {
            IMessageHandlingMember resolver = original;
            foreach (IHandlerEnhancerDefinition enhancer in _enhancers) {
                resolver = enhancer.WrapHandler(type, resolver);
            }
            return resolver;
        }
    }
}