using System;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling
{
    public class DirectEventProcessingStrategy : IEventProcessingStrategy
    {
        private DirectEventProcessingStrategy()
        {
        }

        public DirectEventProcessingStrategy Instance { get; } = new DirectEventProcessingStrategy();
        public void Handle(List<IEventMessage> events, Action<List<IEventMessage>> processor)
        {
            processor(events);
        }
    }
}