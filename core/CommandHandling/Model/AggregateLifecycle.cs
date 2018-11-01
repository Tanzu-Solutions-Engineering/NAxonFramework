using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.Model
{
    public abstract class AggregateLifecycle
    {
        private static AsyncLocal<AggregateLifecycle> Current = new AsyncLocal<AggregateLifecycle>();

        public static IApplyMore Apply(object payload, MetaData metaData) 
        {
            return Instance.DoApply(payload, metaData);
        }
        public static IApplyMore Apply(object payload) 
        {
            return Instance.DoApply(payload, MetaData.EmptyInstance);
        }

        public static IAggregate<T> CreateNew<T>(Func<T> factoryMethod)
        {
            if (!Instance.IsLive)
            {
                throw new NotSupportedException("Aggregate is still initializing its state, creation of new aggregates is not possible");
            }
            return Instance.DoCreateNew(factoryMethod);            
        }
//        public static bool IsLive => Instance.GetIsLive;
//        public static long? GetVersion => Instance.Version;
//        public static void MarkDeleted() => Instance.DoMarkDeleted();

        protected static AggregateLifecycle Instance
        {
            get
            {
                var instance = Current.Value;
                if (instance == null && CurrentUnitOfWork.IsStarted)
                {
                    var unitOfWork = CurrentUnitOfWork.Get();
                    var managedAggregates = unitOfWork.GetResource<ISet<AggregateLifecycle>>("ManagedAggregates");
                    if (managedAggregates != null && managedAggregates.Count == 1)
                    {
                        instance = managedAggregates.First();
                    }

                    if (instance == null)
                    {
                        throw new InvalidOperationException("Cannot retrieve current AggregateLifecycle; none is yet defined");
                    }
                        
                }

                return instance;
            }
        }

        public abstract bool IsLive { get; }
        public abstract long? Version  { get; }
        public abstract void MarkDeleted();

        protected void RegisterWithUnitOfWork()
        {
            CurrentUnitOfWork.IfStarted(u => u.GetOrComputeResource("ManagedAggregates", k => new HashSet<AggregateLifecycle>()).Add(this));
        }

        protected abstract IApplyMore DoApply<T>(T payload, MetaData metaData);
        protected abstract IAggregate<T> DoCreateNew<T>(Func<T> factoryMethod);

        protected V ExecuteWithResult<V>(Func<V> task)
        {
            var handle = RegisterAsCurrent();
            try
            {
                return task.Invoke();
            }
            finally
            {
                handle.Invoke();
            }
        }
        protected Action RegisterAsCurrent() 
        {
            var existing = Current.Value;
            Current.Value = this;
            return () => 
            {
                if (existing == null) 
                {
                    Current.Value = null;
                } 
                else 
                {
                    Current.Value = existing;
                }
            };
        }

        protected void Execute(Action task)
        {
            try
            {
                ExecuteWithResult<object>(() =>
                {
                    task.Invoke();
                    return null;
                });
            }
            catch (Exception e)
            {
                throw new AggregateInvocationException("Exception while invoking a task for an aggregate", e);
            }
        }
    }

    public interface IApplyMore
    {
        IApplyMore AndThenApply<T>(Func<T> payloadOrMessageSupplier);
        IApplyMore AndThen<T>(Action action);
        
    }
}