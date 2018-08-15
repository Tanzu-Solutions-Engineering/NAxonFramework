using System;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling
{
    public class DirectEventProcessingStrategy : IEventProcessingStrategy
    {
        private DirectEventProcessingStrategy()
        {
        }

        public static DirectEventProcessingStrategy Instance { get; } = new DirectEventProcessingStrategy();
        public void Handle(List<IEventMessage> events, Action<List<IEventMessage>> processor)
        {
            processor(events);
        }
    }
}