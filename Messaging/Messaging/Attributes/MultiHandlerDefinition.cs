using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAxonFramework.Messaging.Attributes
{
    public class MultiHandlerDefinition : IHandlerDefinition
    {

        private List<IHandlerDefinition> _handlerDefinitions;
        private readonly IHandlerEnhancerDefinition _handlerEnhancerDefinition;

        public MultiHandlerDefinition(List<IHandlerDefinition> delegates) : this(delegates, ClasspathHandlerEnhancerDefinition.Factory)
        {
        }

        public MultiHandlerDefinition(List<IHandlerDefinition> delegates, IHandlerEnhancerDefinition handlerEnhancerDefinition)
        {
            _handlerDefinitions = Flatten(delegates);
            _handlerEnhancerDefinition = handlerEnhancerDefinition;
        }

        public static MultiHandlerDefinition Ordered(params IHandlerDefinition[] delegates)
        {
            return Ordered(delegates.ToList());
        }
        public static MultiHandlerDefinition Ordered(List<IHandlerDefinition> delegates)
        {
            return new MultiHandlerDefinition(Flatten(delegates));
        }

        private static List<IHandlerDefinition> Flatten(IEnumerable<IHandlerDefinition> factories)
        {
            return factories.SelectMany(x =>
            {
                if (x is MultiHandlerDefinition nested)
                    return nested.Delegates;
                return new[] {x};
            }).ToList();
        }

        public IEnumerable<IHandlerDefinition> Delegates => _handlerDefinitions.AsEnumerable();


        public IMessageHandlingMember<T> CreateHandler<T>(MethodBase executable, IParameterResolverFactory parameterResolverFactory)
        {
            return (IMessageHandlingMember<T>)CreateHandler(typeof(T), executable, parameterResolverFactory);
        }
        public IMessageHandlingMember CreateHandler(Type type, MethodBase executable, IParameterResolverFactory parameterResolverFactory)
        {
            IMessageHandlingMember handler = null;
            foreach (IHandlerDefinition handlerDefinition in _handlerDefinitions) 
            {
                handler = handlerDefinition.CreateHandler(type, executable, parameterResolverFactory);
                if (handler != null) {
                    return _handlerEnhancerDefinition.WrapHandler(type, handler);
                }
            }
            return handler;
        }
        
    }
}