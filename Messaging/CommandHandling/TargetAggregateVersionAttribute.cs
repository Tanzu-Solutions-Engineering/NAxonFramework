using System;

namespace NAxonFramework.CommandHandling
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TargetAggregateVersionAttribute : Attribute
    {
    }
}