using System.Collections;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling.Saga
{
    public interface IAssociationValues : IEnumerable<AssociationValue>
    {
        ISet<AssociationValue> RemovedAssociations { get; }
        ISet<AssociationValue> AddedAssociations { get; }
        void Commit();
        int Size { get; }
        bool Contains(AssociationValue associatedValue);
        bool Add(AssociationValue associatedValue);
        bool Remove(AssociationValue associatedValue);
        ISet<AssociationValue> AsSet();
    }
}