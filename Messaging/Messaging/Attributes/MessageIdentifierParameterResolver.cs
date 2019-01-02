using System;

namespace NAxonFramework.Messaging.Attributes
{
    class MessageIdentifierParameterResolver : IParameterResolver<string>
    {
        public bool Matches(IMessage message) => true;

        public Type SupportedPayloadType => typeof(object);
        public string ResolveParameterValue(IMessage message) => message.Identifier;

        object IParameterResolver.ResolveParameterValue(IMessage message) => ResolveParameterValue(message);
    }
}