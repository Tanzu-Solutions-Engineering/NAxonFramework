using System;

namespace NAxonFramework.Messaging.Attributes
{
    public interface IHandlerEnhancerDefinition
    {
        IMessageHandlingMember<T> WrapHandler<T>(IMessageHandlingMember<T> original);
        IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original);
    }
}