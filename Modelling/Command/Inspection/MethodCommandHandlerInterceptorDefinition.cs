using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;
using MoreLinq;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class MethodCommandHandlerInterceptorDefinition : IHandlerEnhancerDefinition
    {
        public IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original)
        {
            return original.AnnotationAttributes(typeof(CommandHandlerInterceptorAttribute))
                .Map(attr => (IMessageHandlingMember) new MethodCommandHandlerInterceptorHandlingMember(
                    original, attr))
                .OrElse(original);
        }
        
        private class MethodCommandHandlerInterceptorHandlingMember : WrappedMessageHandlingMember, ICommandHandlerInterceptorHandlingMember 
        {

        private readonly Regex _commandNamePattern;
        private readonly bool _shouldInvokeInterceptorChain;

        public MethodCommandHandlerInterceptorHandlingMember(IMessageHandlingMember @delegate,
            IDictionary<string, object> annotationAttributes) : base(@delegate)
        {
            var method = @delegate.Unwrap<MethodBase>().OrElseThrow(() => new AxonConfigurationException(
                "The @CommandHandlerInterceptor must be on method."));
            
           

            _shouldInvokeInterceptorChain = method.GetParameters().All(p => p.ParameterType != typeof(IInterceptorChain));
            if (_shouldInvokeInterceptorChain && (method as MethodInfo)?.ReturnType != null) 
            {
                throw new AxonConfigurationException("@CommandHandlerInterceptor must return void or declare InterceptorChain parameter.");
            }
            _commandNamePattern = new Regex((string) annotationAttributes.GetValueOrDefault("commandNamePattern"));
        }

            public override bool CanHandle(IMessage message)
            {
                return base.CanHandle(message) && _commandNamePattern.IsMatch(((ICommandMessage) message).CommandName);
            }

            public bool ShouldInvokeInterceptorChain() => _shouldInvokeInterceptorChain;
        }
    }
}