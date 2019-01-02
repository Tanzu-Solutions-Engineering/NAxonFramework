using System;

namespace NAxonFramework.CommandHandling.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    [EntityId]
    public class AggregateIdentifierAttribute : Attribute
    {
        public AggregateIdentifierAttribute(string routingKey)
        {
            RoutingKey = routingKey;
        }

        private string RoutingKey { get; set; }
    }
}