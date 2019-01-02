using System;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling
{
    public interface IEventProcessingStrategy
    {
        void Handle(List<IEventMessage> events, Action<List<IEventMessage>> processor);
    }
}