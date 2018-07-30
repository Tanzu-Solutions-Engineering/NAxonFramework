namespace NAxonFramework.EventHandling
{
    public enum ReplayStatus
    {
        Replay,
        Regular
    }

    public static class ReplayStatusExtensions
    {
        public static bool IsReplay(this ReplayStatus replayStatus)
        {
            return replayStatus == ReplayStatus.Replay;
        }
    }
}