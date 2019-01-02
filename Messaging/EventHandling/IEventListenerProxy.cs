using System;

namespace NAxonFramework.EventHandling
{
    public interface IEventListenerProxy : IEventListener
    {
        Type TargetType { get; }
    }
}