using System;
using System.Collections.Generic;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public interface ISagaStore
    {
        ISet<string> FindSagas(Type sagaType, AssociationValue associationValue);
        IEntry LoadSaga(Type sagaType, string sagaIdentifier);
        void DeleteSaga(Type sagaType, string sagaIdentifier, ISet<AssociationValue> associationValues);
        void InsertSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, ISet<AssociationValue> associationValues);
        void UpdateSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, IAssociationValues associationValues);
        
    }
}