using System;

namespace NAxonFramework.EventHandling
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Method)]
    public class AllowReplayAttribute : Attribute
    {
        private bool Value { get; set; }

        public AllowReplayAttribute(bool value = true)
        {
            Value = value;
        }
    }
}