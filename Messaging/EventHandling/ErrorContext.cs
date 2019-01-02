using System;
using System.Collections;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling
{
    public class ErrorContext
    {
        public string EventProcessor { get; }
        public Exception Error { get; }
        public List<IEventMessage> FailedEvents { get; }

        public ErrorContext(string eventProcessor, Exception error, List<IEventMessage> failedEvents)
        {
            EventProcessor = eventProcessor;
            Error = error;
            FailedEvents = failedEvents;
        }
    }
}