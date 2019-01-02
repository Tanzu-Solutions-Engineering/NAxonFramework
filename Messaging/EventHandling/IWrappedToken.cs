namespace NAxonFramework.EventHandling
{
    public interface IWrappedToken
    {
        ITrackingToken Unwrap();
    }
}