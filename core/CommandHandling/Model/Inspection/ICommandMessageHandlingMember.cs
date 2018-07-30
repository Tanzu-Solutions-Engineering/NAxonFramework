namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface ICommandMessageHandlingMember
    {
        string CommandName { get; }
        string RoutingKey { get; }
        bool IsFactoryHandler { get; }
    }
}