using System;

namespace NAxonFramework.Messaging.Attributes
{
    public class MessageIdentifierParameterResolverFactory : AbstractAnnotatedParameterResolverFactory<MessageIdentifier,string>
    {
        private readonly IParameterResolver<String> _resolver = new MessageIdentifierParameterResolver();

        protected override IParameterResolver<string> GetResolver() => _resolver;
    }

    class MessageIdentifierParameterResolver : IParameterResolver<string>
    {
        public bool Matches(IMessage message) => true;

        public Type SupportedPayloadType => typeof(object);
        public string ResolveParameterValue(IMessage message) => message.Identifier;

        object IParameterResolver.ResolveParameterValue(IMessage message) => ResolveParameterValue(message);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class MessageIdentifier : Attribute
    {
        
    }
}