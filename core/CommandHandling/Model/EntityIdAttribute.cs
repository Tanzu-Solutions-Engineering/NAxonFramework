using System;

namespace NAxonFramework.CommandHandling.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityIdAttribute : Attribute
    {
        public EntityIdAttribute(string routingKey = "")
        {
            RoutingKey = routingKey;
        }

        public string RoutingKey { get; } 
    }
}