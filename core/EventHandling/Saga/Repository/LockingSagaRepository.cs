using System;
using System.Collections.Generic;
using NAxonFramework.Common.Lock;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public abstract class LockingSagaRepository<T> : ISagaRepository<T>
    {
        private readonly ILockFactory _lockFactory;

        protected LockingSagaRepository(ILockFactory lockFactory)
        {
            _lockFactory = lockFactory;
        }

        public ISaga<T> Load(string sagaIdentifier)
        {
            LockSagaAccess(sagaIdentifier);
            return DoLoad(sagaIdentifier);
        }

        public abstract ISet<string> Find(AssociationValue associationValue);

        public ISaga<T> CreateInstance(string sagaIdentifier, Func<T> factoryMethod)
        {
            LockSagaAccess(sagaIdentifier);
            return DoCreateInstance(sagaIdentifier, factoryMethod);
        }
        
        private void LockSagaAccess(string sagaIdentifier) 
        {
            var unitOfWork = CurrentUnitOfWork.Get();
            var @lock = _lockFactory.ObtainLock(sagaIdentifier);
            unitOfWork.Root().OnCleanup(u => @lock.Dispose());
        }

        protected abstract ISaga<T> DoLoad(string sagaIdentifier);
        protected abstract ISaga<T> DoCreateInstance(string sagaIdentifier, Func<T> factoryMethod);
    }
}