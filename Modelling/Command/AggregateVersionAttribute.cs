using System;

namespace NAxonFramework.CommandHandling.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AggregateVersionAttribute : Attribute
    {
        
    }
}