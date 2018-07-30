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
        void OnPrepareCommit(Action<IUnitOfWork> handler);
        void OnCommit(Action<IUnitOfWork> handler);
        void AfterCommit(Action<IUnitOfWork> handler);
        void OnRollback(Action<IUnitOfWork> handler);
        void OnCleanup(Action<IUnitOfWork> handler);
        Optional<IUnitOfWork> Parent { get; }
        IMessage Message { get; }
        IUnitOfWork TransformMessage(Func<IMessage, IMessage> transformOperation);
        MetaData CorrelationData { get; }
        void RegisterCorrelationDataProvider(ICorrelationDataProvider correlationDataProvider);
        IDictionary<string,object> Resources { get; }
        R ExecuteWithResult<R>(Func<R> task, RollbackConfigurationType rollbackConfiguration);
        Task<object> ExecutionResult { get; }
        bool IsRolledBack { get; }
        
    }
}