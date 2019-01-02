namespace NAxonFramework.EventHandling.Async
{
    public interface ISequencingPolicy<T>
    {
        object GetSequenceIdentifierFor(T @event);
    }
}