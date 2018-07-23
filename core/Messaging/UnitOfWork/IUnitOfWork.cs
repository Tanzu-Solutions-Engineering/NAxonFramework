using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Correlation;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public interface IUnitOfWork
    {
        void Start();
        void Commit();
        void Rollback(Exception exception = null);
        Phase Phase { get; }
        void OnPrepareCommit(Func<IUnitOfWork> handler);
        void OnCommit(Func<IUnitOfWork> handler);
        void AfterCommit(Func<IUnitOfWork> handler);
        void OnRollback(Func<IUnitOfWork> handler);
        void OnCleanup(Func<IUnitOfWork> handler);
        Optional<IUnitOfWork> Parent { get; }
        IMessage Message { get; }
        IUnitOfWork TransformMessage(Func<IMessage> transformOperation);
        MetaData CorrelationData { get; }
        void RegisterCorrelationDataProvider(ICorrelationDataProvider correlationDataProvider);
        IDictionary<string,object> Resources { get; }
        R ExecuteWithResult<R>(Func<R> task, RollbackConfigurationType rollbackConfiguration);
        Task<object> ExecutionResult { get; }
        bool IsRolledBack { get; }
        
    }

    public static class UnitOfWorkExtensions
    {
        public static bool IsActive(this IUnitOfWork unitOfWork) => unitOfWork.Phase.IsStarted();
        public static bool IsRoot(this IUnitOfWork unitOfWork) => !unitOfWork.Parent.IsPresent;
        public static IUnitOfWork Root(this IUnitOfWork unitOfWork) => unitOfWork.Parent.Map(x => x.Root()).OrElse(unitOfWork);
        public static R GetResource<R>(this IUnitOfWork unitOfWork, string name) => (R)unitOfWork.Resources.GetValueOrDefault(name);
        public static R GetOrComputeResource<R>(this IUnitOfWork unitOfWork, string key, Func<string, R> mappingFunction) 
            => (R)unitOfWork.Resources.ComputeIfAbsent(key, k => mappingFunction(k));

        public static R GetOrDefaultResource<R>(this IUnitOfWork unitOfWork, string key, R defaultValue)
            => (R)unitOfWork.Resources.GetValueOrDefault(key, defaultValue);
        public static void Execute(this IUnitOfWork unitOfWork, Action task) => unitOfWork.Execute(task, RollbackConfigurationType.AnyThrowable);

        public static void Execute(this IUnitOfWork unitOfWork, Action task, RollbackConfigurationType rollbackConfiguration)
            => unitOfWork.ExecuteWithResult(() =>
            {
                task.Invoke();
                return (object)null;
            }, rollbackConfiguration);

        public static R ExecuteWithResult<R>(this IUnitOfWork unitOfWork, Func<R> task) => unitOfWork.ExecuteWithResult(task, RollbackConfigurationType.AnyThrowable);
        public static bool IsCurrent(this IUnitOfWork unitOfWork) => CurrentUnitOfWork.IsStarted && CurrentUnitOfWork.Get() == unitOfWork;
        
    }

    public enum RollbackConfigurationType
    {
        Never,
        AnyThrowable,
        UncheckedException,
        RuntimeExceptions
    }

    public enum Phase
    {
        NOT_STARTED,
        STARTED, 
        PREPARE_COMMIT, 
        COMMIT, 
        ROLLBACK, 
        AFTER_COMMIT, 
        CLEANUP, 
        CLOSED 
        
    }

    public static class PhaseExtensions
    {
        public static bool IsStarted(this Phase phase)
        {
            
            switch (phase)
            {
                case Phase.NOT_STARTED:
                    return false;
                case Phase.STARTED:
                    return true;
                case Phase.PREPARE_COMMIT:
                    return true;
                case Phase.COMMIT:
                    return true;
                case Phase.ROLLBACK:
                    return true;
                case Phase.AFTER_COMMIT:
                    return true;
                case Phase.CLEANUP:
                    return false;
                case Phase.CLOSED:
                    return false;
                default:
                    return false;
            }
        }
        public static bool IsReverseCallbackOrder(this Phase phase)
        {
            switch (phase)
            {
                case Phase.NOT_STARTED:
                    return false;
                case Phase.STARTED:
                    return false;
                case Phase.PREPARE_COMMIT:
                    return false;
                case Phase.COMMIT:
                    return true;
                case Phase.ROLLBACK:
                    return true;
                case Phase.AFTER_COMMIT:
                    return true;
                case Phase.CLEANUP:
                    return true;
                case Phase.CLOSED:
                    return true;
                default:
                    return true;
            }
        }

        public static bool IsBefore(this Phase phase, Phase otherPhase) => phase < otherPhase;
        public static bool IsAfter(this Phase phase, Phase otherPhase) => phase > otherPhase;
    }
}