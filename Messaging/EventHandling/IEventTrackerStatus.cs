namespace NAxonFramework.EventHandling
{
    public interface IEventTrackerStatus
    {
        Segment Segment { get; }
        bool IsCaughtUp { get; }
        bool IsReplaying { get; }
        ITrackingToken TrackingToken { get; }
    }
}