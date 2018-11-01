using System;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling
{
    public class InterceptorChainParameterResolverFactory : IParameterResolverFactory, IParameterResolver<IInterceptorChain>
    {
        private static String INTERCEPTOR_CHAIN_EMITTER_KEY = nameof(IInterceptorChain);
        
        public static void Initialize(IInterceptorChain interceptorChain)
        {
            Assert.State(CurrentUnitOfWork.IsStarted,
                () => "An active Unit of Work is required for injecting interceptor chain");
            CurrentUnitOfWork.Get().Resources[INTERCEPTOR_CHAIN_EMITTER_KEY] = interceptorChain;
        }

        public IInterceptorChain ResolveParameterValue(IMessage message)
        {
            return CurrentUnitOfWork.Map(uow => uow.GetResource<IInterceptorChain>(INTERCEPTOR_CHAIN_EMITTER_KEY))
                .OrElseThrow(() => new InvalidOperationException("InterceptorChain should have been injected"));
        }

        public bool Matches(IMessage message) => message is ICommandMessage;

        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            if (parameters[parameterIndex].ParameterType == typeof(IInterceptorChain)) 
            {
                return this;
            }
            return null;
        }

        public Type SupportedPayloadType => typeof(object);

        object IParameterResolver.ResolveParameterValue(IMessage message)
        {
            return ResolveParameterValue(message);
        }
    }
}