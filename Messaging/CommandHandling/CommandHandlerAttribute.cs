using System;

namespace NAxonFramework.CommandHandling
{
    public class CommandHandlerAttribute : Attribute
    {
        public CommandHandlerAttribute(string commandName, string routingKey, Type payloadType)
        {
            CommandName = commandName;
            RoutingKey = routingKey;
            PayloadType = payloadType;
        }

        

        public string CommandName { get; } = "";
        public string RoutingKey { get; } = "";
        public Type PayloadType { get; } = typeof(object);
    }
}