using System;

namespace NAxonFramework.CommandHandling.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AggregateMemberAttribute
    {
        public bool ForwardCommands { get; set; } = true;
        public Type EventForwardingMode { get; set; } = typeof(ForwardToAll);
        public string RoutingKey { get; set; } = string.Empty;
        public Type Type { get; set; }
    }
}