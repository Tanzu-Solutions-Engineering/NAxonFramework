using System;

namespace NAxonFramework.EventHandling.Saga
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StartSagaAttribute : Attribute
    {
        
    }
}