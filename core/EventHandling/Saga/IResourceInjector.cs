using System;

namespace NAxonFramework.EventHandling.Saga
{
    public interface IResourceInjector
    {
        void InjectResources(object saga);
    }

    public interface ISaga<T> : IEventListener
    {
        string SagaIdentifier { get; }
        IAssociationValues AssociationValues { get; }
        
        R Invoke<R>(Func<T, R> invocation);
        void Execute(Action<T> invocation);
        bool IsActive { get; }
        ITrackingToken TrackingToken { get; }
        bool SupportsReset { get; }
        void PrepareReset();
    }
}