using System;

namespace NAxonFramework.Messaging.Attributes
{
    public interface IHandlerEnhancerDefinition
    {
//        IMessageHandlingMember WrapHandler(IMessageHandlingMember original);
        IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original);
    }
}