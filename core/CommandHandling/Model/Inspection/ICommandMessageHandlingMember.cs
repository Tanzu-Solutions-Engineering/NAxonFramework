using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface ICommandMessageHandlingMember : IMessageHandlingMember
    {
        string CommandName { get; }
        string RoutingKey { get; }
        bool IsFactoryHandler { get; }
    }
}