namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IAggregateModel : IEntityModel
    {
        string Type { get; }
        long? GetVersion(object target);
    }
}