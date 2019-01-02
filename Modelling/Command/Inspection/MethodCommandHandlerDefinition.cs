using System;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class MethodCommandHandlerDefinition : IHandlerEnhancerDefinition
    {
        public IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original)
        {
            return original.AnnotationAttributes(typeof(CommandHandlerAttribute))
                .Map(attr => (IMessageHandlingMember) new MethodCommandMessageHandlingMember(original, attr))
                .OrElse(original);
        }

        private class MethodCommandMessageHandlingMember : WrappedMessageHandlingMember, ICommandMessageHandlingMember
        {
            public MethodCommandMessageHandlingMember(IMessageHandlingMember @delegate,
                IDictionary<String, Object> annotationAttributes) : base(@delegate)
            {

                RoutingKey = string.IsNullOrEmpty((string) annotationAttributes.GetValueOrDefault("routingKey"))
                    ? null
                    : (string) annotationAttributes.GetValueOrDefault("routingKey");


                var executable = @delegate.Unwrap<MethodBase>().OrElseThrow(() => new AxonConfigurationException(
                    "The @CommandHandler annotation must be put on an Executable (either directly or as Meta " +
                    "Annotation)"));
                if (string.IsNullOrEmpty((string) annotationAttributes.GetValueOrDefault("commandName")))
                {
                    CommandName = @delegate.PayloadType.Name;
                }
                else
                {
                    CommandName = (string) annotationAttributes.GetValueOrDefault("commandName");
                }

                var factoryMethod = executable is MethodInfo && executable.IsStatic;
                if (factoryMethod && !executable.DeclaringType.IsAssignableFrom(((MethodInfo) executable).ReturnType))
                {
                    throw new AxonConfigurationException("static @CommandHandler methods must declare a return value " +
                                                         "which is equal to or a subclass of the declaring time");
                }

                IsFactoryHandler = executable is ConstructorInfo || factoryMethod;
            }

            public override bool CanHandle(IMessage message)
            {
                return base.CanHandle(message) && CommandName.Equals(((ICommandMessage) message).CommandName);
            }

            public string RoutingKey { get; }

            public string CommandName { get; }

            public bool IsFactoryHandler { get; }
        }
    }
}