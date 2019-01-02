using System;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling
{
    [Priority(Priority.High)]
    public class TimestampParameterResolverFactory : AbstractAnnotatedParameterResolverFactory<TimestampAttribute, DateTime?>
    {
        private readonly IParameterResolver<DateTime?> _resolver;

        public TimestampParameterResolverFactory()
        {
            _resolver = new TimestampParameterResolver();
        }

        protected override IParameterResolver<DateTime?> GetResolver() => _resolver;
        
        
        private class TimestampParameterResolver : IParameterResolver<DateTime?> 
        {
            
            public bool Matches(IMessage message)
            {
                return message is IEventMessage;
            }

            public Type SupportedPayloadType => typeof(object);
            public DateTime? ResolveParameterValue(IMessage message)
            {
                if (message is IEventMessage eventMessage)
                    return eventMessage.Timestamp;
                return null;
            }

            object IParameterResolver.ResolveParameterValue(IMessage message)
            {
                return ResolveParameterValue(message);
            }
        }
    }
    
    
}