using System;
using System.Threading;

namespace NAxonFramework.EventHandling.Saga
{
    public abstract class SagaLifecycle
    {
        private static readonly AsyncLocal<SagaLifecycle> CurrentSagaLifecycle = new AsyncLocal<SagaLifecycle>();

        public static void AssociateWith(string associationKey, string associationValue)
        {
            AssociateWith(new AssociationValue(associationKey, associationValue));
        }


        public static void AssociateWith(AssociationValue associationValue)
        {
            Instance.DoAssociateWith(associationValue);
        }


        public static void RemoveAssociationWith(string associationKey, string associationValue)
        {
            Instance.DoRemoveAssociation(new AssociationValue(associationKey, associationValue));
        }

        public static void End()
        {
            Instance.DoEnd();
        }


        protected abstract void DoEnd();
        protected abstract void DoRemoveAssociation(AssociationValue associationValue);
        protected abstract void DoAssociateWith(AssociationValue associationValue);

        protected static SagaLifecycle Instance
        {
            get
            {
                SagaLifecycle instance = CurrentSagaLifecycle.Value;
                if (instance == null)
                {
                    throw new InvalidOperationException("Cannot retrieve current SagaLifecycle; none is yet defined");
                }

                return instance;
            }
        }

        protected V ExecuteWithResult<V>(Func<V> task)
        {
            var existing = CurrentSagaLifecycle.Value;
            CurrentSagaLifecycle.Value = this;
            return task.Invoke();
        }

        protected void Execute(Action task)
        {
            try
            {
                ExecuteWithResult(() =>
                {
                    task.Invoke();
                    return (object) null;
                });
            }
            catch (ApplicationException e)
            {
                throw new SagaExecutionException("Exception while executing a task for a saga", e);
            }
        }
    }
}