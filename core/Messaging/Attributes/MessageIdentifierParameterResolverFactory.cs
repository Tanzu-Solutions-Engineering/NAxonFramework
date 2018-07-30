using System;

namespace NAxonFramework.Messaging.Attributes
{
    public class MessageIdentifierParameterResolverFactory : AbstractAnnotatedParameterResolverFactory<MessageIdentifier,string>
    {
        private readonly IParameterResolver<String> _resolver = new MessageIdentifierParameterResolver();

        protected override IParameterResolver<string> GetResolver() => _resolver;
    }
}