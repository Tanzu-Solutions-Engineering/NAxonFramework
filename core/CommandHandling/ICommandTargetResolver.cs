namespace NAxonFramework.CommandHandling
{
    public interface ICommandTargetResolver
    {
        VersionedAggregateIdentifier ResolveTarget(ICommandMessage command);
    }
}