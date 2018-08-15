using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using NAxonFramework.Common;
using NAxonFramework.Common.Lock;
using NAxonFramework.EventHandling.Saga.MetaModel;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public class AnnotatedSagaRepository<T> : LockingSagaRepository<T>
    {
        
        private readonly string _unsavedSagasResourceKey;
        private readonly Dictionary<String, AnnotatedSaga<T>> _managedSagas;
        private readonly ISagaStore _sagaStore;
        private readonly ISagaModel _sagaModel;
//        private readonly ResourceInjector injector;
        
        public AnnotatedSagaRepository(ISagaStore sagaStore, ISagaModel sagaModel, ILockFactory lockFactory) : base(lockFactory)
        {
            _sagaStore = sagaStore;
            _sagaModel = sagaModel;
        }

        protected override ISaga<T> DoLoad(string sagaIdentifier)
        {
            var unitOfWork = CurrentUnitOfWork.Get();
            var processRoot = unitOfWork.Root();

            var loadedSaga = _managedSagas.ComputeIfAbsent(sagaIdentifier, id =>
            {
                var result = DoLoadSaga(sagaIdentifier);
                if (result != null)
                {
                    processRoot.OnCleanup(u => _managedSagas.Remove(id));
                }

                return result;
            });
            if (loadedSaga != null && UnsavedSagasResource(processRoot).Add(sagaIdentifier))
            {
                unitOfWork.OnPrepareCommit(u =>
                {
                    UnsavedSagasResource(processRoot).Remove(sagaIdentifier);
                    Commit(loadedSaga);
                });
            }

            return loadedSaga;
        }

        protected override ISaga<T> DoCreateInstance(string sagaIdentifier, Func<T> sagaFactory)
        {
            try
            {
                var unitOfWOrk = CurrentUnitOfWork.Get();
                var processRoot = unitOfWOrk.Root();
                var sagaRoot = sagaFactory.Invoke();
                var saga = new AnnotatedSaga<T>(sagaIdentifier, new HashSet<AssociationValue>(), sagaRoot, null, _sagaModel );
                UnsavedSagasResource(processRoot).Add(sagaIdentifier);
                unitOfWOrk.OnPrepareCommit(u =>
                {
                    if (saga.IsActive)
                    {
                        StoreSaga(saga);
                        saga.AssociationValues.Commit();
                        UnsavedSagasResource(processRoot).Remove(sagaIdentifier);
                    }
                });
                _managedSagas[sagaIdentifier] = saga;
                processRoot.OnCleanup(u => _managedSagas.Remove(sagaIdentifier));
                return saga;
            }
            catch (Exception e)
            {
                throw new SagaCreationException("An error occurred while attempting to create a new managed instance", e);
            }
        }

        private HashSet<string> UnsavedSagasResource(IUnitOfWork unitOfWork)
        {
            return unitOfWork.GetOrComputeResource(_unsavedSagasResourceKey, i => new HashSet<string>());
        }

        private void Commit(AnnotatedSaga<T> saga)
        {
            if (!saga.IsActive)
            {
                DeleteSaga(saga);
            }
            else
            {
                UpdateSaga(saga);
                saga.AssociationValues.Commit();
            }
        }

        public override ISet<string> Find(AssociationValue associationValue)
        {
            var sagasFound = new SortedSet<string>();
            _managedSagas.Values.Where(saga => saga.AssociationValues.Contains(associationValue))
                .Select(x => x.SagaIdentifier)
                .ForEach(x => sagasFound.Add(x));
            _sagaStore.FindSagas(typeof(T), associationValue).ForEach(x => sagasFound.Add(x));
            return sagasFound;
        }

        private void DeleteSaga(AnnotatedSaga<T> saga)
        {
            var associationValues = saga.AssociationValues.ToHashSet().Union(saga.AssociationValues.RemovedAssociations).ToHashSet();
            _sagaStore.DeleteSaga(typeof(T), saga.SagaIdentifier, associationValues);
            
        }

        private void UpdateSaga(AnnotatedSaga<T> saga)
        {
            _sagaStore.UpdateSaga(typeof(T), saga.SagaIdentifier, saga.Root, saga.TrackingToken, saga.AssociationValues);
        }

        private void StoreSaga(AnnotatedSaga<T> saga)
        {
            _sagaStore.InsertSaga(typeof(T), saga.SagaIdentifier, saga.Root, saga.TrackingToken, saga.AssociationValues.AsSet());
        }

        protected AnnotatedSaga<T> DoLoadSaga(string sagaIdentifier)
        {
            var entry = _sagaStore.LoadSaga(typeof(T), sagaIdentifier);
            if (entry != null)
            {
                var saga = (T)entry.Saga;
                return new AnnotatedSaga<T>(sagaIdentifier, entry.AssociationValues, saga, entry.TrackingToken, _sagaModel);
            }

            return null;
        }
    }
}