namespace NAxonFramework.EventHandling
{
    public interface ITrackingToken
    {
        ITrackingToken LowerBound(ITrackingToken other);
        ITrackingToken UpperBound(ITrackingToken other);
        bool Covers(ITrackingToken other);
    }
}