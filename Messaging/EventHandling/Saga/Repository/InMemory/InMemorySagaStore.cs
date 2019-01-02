using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using MoreLinq.Extensions;

namespace NAxonFramework.EventHandling.Saga.Repository.InMemory
{
    public class InMemorySagaStore : ISagaStore
    {
        private readonly ConcurrentDictionary<string, ManagedSaga> _managedSagas = new ConcurrentDictionary<string, ManagedSaga>();
        public ISet<string> FindSagas(Type sagaType, AssociationValue associationValue)
        {
            return _managedSagas.Where(avEntry => sagaType.IsInstanceOfType(avEntry.Value.Saga))
                .Where(avEntry => avEntry.Value.AssociationValues.Contains(associationValue))
                .Select(x => x.Key)
                .ToHashSet();

        }

        public void DeleteSaga(Type sagaType, string sagaIdentifier, ISet<AssociationValue> associationValues)
        {
            _managedSagas.TryRemove(sagaIdentifier, out _);
        }

        public IEntry LoadSaga(Type sagaType, string sagaIdentifier)
        {
            return _managedSagas.TryGetValue(sagaIdentifier, out var saga) ? saga : null;
        }

        public void InsertSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, ISet<AssociationValue> associationValues)
        {
            _managedSagas.AddOrUpdate(sagaIdentifier, new ManagedSaga(saga, associationValues), (s, managedSaga) => new ManagedSaga(saga, associationValues));
        }

        public void UpdateSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, IAssociationValues associationValues)
        {
            _managedSagas.AddOrUpdate(sagaIdentifier, new ManagedSaga(saga, associationValues.AsSet()), (s, managedSaga) => new ManagedSaga(saga, associationValues.AsSet()));
        }

        public int Count => _managedSagas.Count;

        private class ManagedSaga : IEntry
        {

            public ManagedSaga(object saga, ISet<AssociationValue> associationValues)
            {
                Saga = saga;
                AssociationValues = associationValues;
            }


            public ITrackingToken TrackingToken => null;
            public ISet<AssociationValue> AssociationValues { get; }
            public object Saga { get; }
        }
    }
}