using System;

namespace NAxonFramework.CommandHandling.Gateway
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TimeoutAttribute : Attribute
    {
        public TimeoutAttribute(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}