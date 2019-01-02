using System;

namespace NAxonFramework.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        public Priority Priority { get; }

        public PriorityAttribute(Priority priority = Priority.Neutral)
        {
            Priority = priority;
        }
    }
    
    public enum Priority
    {
        Last = Int32.MinValue,
        Low = Int32.MinValue / 2,
        Neutral = 0,
        High = Int32.MaxValue / 2,
        First = Int32.MaxValue, 
    }
}