using System;
using System.Collections;
using System.Collections.Generic;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IChildEntity
    {
        void Publish(IEventMessage msg, object declaringInstance);
        IReadOnlyDictionary<string, IMessageHandlingMember> CommandHandlers { get; }
    }
}