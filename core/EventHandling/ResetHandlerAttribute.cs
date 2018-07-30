using System;

namespace NAxonFramework.EventHandling
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [EventHandler(payloadType: typeof(ResetTriggerEvent))]
    public class ResetHandlerAttribute : Attribute
    {
        
    }
}