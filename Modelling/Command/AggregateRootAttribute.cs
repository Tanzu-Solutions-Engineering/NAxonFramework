using System;

namespace NAxonFramework.CommandHandling.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class AggregateRootAttribute : Attribute
    {
        public string Type { get; set; }
    }
}