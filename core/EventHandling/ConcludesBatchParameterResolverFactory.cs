using System;
using System.Globalization;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.EventHandling
{
    public class ConcludesBatchParameterResolverFactory : AbstractAnnotatedParameterResolverFactory<ConcludesBatchAttribute, bool>, IParameterResolver<bool>
    {
        
        protected override IParameterResolver<bool> GetResolver() => this;
        public bool Matches(IMessage message) => message is IEventMessage;

        public Type SupportedPayloadType { get; }
        public bool ResolveParameterValue(IMessage message)
        {
            return CurrentUnitOfWork.Map(unitOfWork => !(unitOfWork is BatchingUnitOfWork) || ((BatchingUnitOfWork) unitOfWork).IsLastMessage(message))
                .OrElse(true);
        }

        object IParameterResolver.ResolveParameterValue(IMessage message)
        {
            return ResolveParameterValue(message);
        }
    }
}