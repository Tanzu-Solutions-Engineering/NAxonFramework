using System.Collections.Generic;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public interface IEntry
    {
        ITrackingToken TrackingToken { get; }
        ISet<AssociationValue> AssociationValues { get; }
        object Saga { get; }
    }
}