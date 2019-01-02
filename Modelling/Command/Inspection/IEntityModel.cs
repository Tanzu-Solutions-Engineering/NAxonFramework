using System;
using System.Collections.Generic;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IEntityModel
    {
        object GetIdentifier(object target);

        string RoutingKey { get; }

        void Publish(IEventMessage message, object target);

        IList<IMessageHandlingMember> CommandHandlers { get; }

        List<IMessageHandlingMember> CommandHandlerInterceptors { get; }

        IEntityModel ModelOf(Type childEntityType);

        Type EntityClass { get; }
    }
}