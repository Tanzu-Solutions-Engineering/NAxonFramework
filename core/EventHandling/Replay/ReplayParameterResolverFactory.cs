using System;
using System.Reflection;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Replay
{
    public class ReplayParameterResolverFactory : IParameterResolverFactory
    {
        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            if (typeof(ReplayStatus).IsAssignableFrom(parameters[parameterIndex].ParameterType))
            {
                return new ReplayParameterResolver();
            }

            return null;
        }

        private class ReplayParameterResolver : IParameterResolver
        {
            public bool Matches(IMessage message) => true;

            public Type SupportedPayloadType => typeof(object);
            public object ResolveParameterValue(IMessage message)
            {
                return message is ITrackedEventMessage trackedEventMessage
                    && trackedEventMessage.TrackingToken is ReplayToken
                    ? ReplayStatus.Replay
                    : ReplayStatus.Regular;
            }
        }
    }
}