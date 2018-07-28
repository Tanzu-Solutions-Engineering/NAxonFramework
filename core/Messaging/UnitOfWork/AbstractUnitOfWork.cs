using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Correlation;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public abstract class AbstractUnitOfWork : IUnitOfWork
    {
        private readonly ILogger<AbstractUnitOfWork> _logger = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILogger<AbstractUnitOfWork>>();
        private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();
        private readonly HashSet<ICorrelationDataProvider> _correlationDataProviders = new HashSet<ICorrelationDataProvider>();
        private IUnitOfWork _parentUnitOfWork;
        private Phase _phase = Phase.NOT_STARTED;
        private bool _rolledBack;


        public void Start()
        {
            
            _logger.LogDebug("Starting Unit of Work");
            Assert.State(_phase == Phase.NOT_STARTED, () => "UnitOfWork is already started");
            _rolledBack = false;
            OnRollback(u => _rolledBack = true);
            CurrentUnitOfWork.IfStarted(parent =>
            {
                _parentUnitOfWork = parent;
                this.Root().OnCleanup(r => ChangePhase(Phase.CLEANUP, Phase.CLOSED));
            });
            ChangePhase(Phase.STARTED);
            CurrentUnitOfWork.Set(this);
            
        }
        
        public void Commit()
        {
            _logger.LogDebug("Committing Unit Of Work");
            Assert.State(Phase == Phase.STARTED, () => $"The UnitOfWork is in an incompatible phase: {Phase}");
            Assert.State(this.IsCurrent(), () => "The UnitOfWork is not the current Unit of Work");
            try
            {
                if (this.IsRoot())
                {
                    CommitAsRoot();
                }
                else
                {
                    CommitAsNested();
                }
            }
            finally
            {
                CurrentUnitOfWork.Clear(this);
            }
        }

        private void CommitAsRoot()
        {
            try
            {
                try
                {
                    ChangePhase(Phase.PREPARE_COMMIT, Phase.COMMIT);
                }
                catch (Exception e)
                {
                    SetRollbackCause(e);
                    ChangePhase(Phase.ROLLBACK);
                    throw e;
                }

                if (Phase == Phase.COMMIT)
                {
                    ChangePhase(Phase.AFTER_COMMIT);
                }
            }
            finally
            {
                ChangePhase(Phase.CLEANUP, Phase.CLOSED);
            }
        }

        private void CommitAsNested()
        {
            try
            {
                ChangePhase(Phase.PREPARE_COMMIT, Phase.COMMIT);
                DelegateAfterCommitToParent(this);
                _parentUnitOfWork.OnRollback(u => ChangePhase(Phase.ROLLBACK));
            }
            catch (Exception e)
            {
                SetRollbackCause(e);
                ChangePhase(Phase.ROLLBACK);
                throw;
            }
        }

        private void DelegateAfterCommitToParent(IUnitOfWork uow)
        {
            var parent = uow.Parent;
            if (parent.IsPresent)
            {
                parent.Get().AfterCommit(DelegateAfterCommitToParent);
            }
            else
            {
                ChangePhase(Phase.AFTER_COMMIT);
            }
        }
        

        public void Rollback(Exception cause = null)
        {
            _logger.LogDebug("Rolling back Unit Of Work.", cause);
            Assert.State(this.IsActive() && Phase.IsBefore(Phase.ROLLBACK), () => $"The UnitOfWork is in an incompatible phase: {Phase}");
            Assert.State(this.IsCurrent() , () => "The UnitOfWork is not the current Unit of Work");
            try
            {
                this.SetRollbackCause(cause);
                ChangePhase(Phase.ROLLBACK);
                if (this.IsRoot())
                {
                    ChangePhase(Phase.CLEANUP, Phase.CLOSED);
                }
            }
            finally
            {
                CurrentUnitOfWork.Clear(this);
            }
        }

        public Optional<IUnitOfWork> Parent => Optional<IUnitOfWork>.OfNullable(_parentUnitOfWork);
        public IDictionary<string, object> Resources => _resources;
        public bool IsRolledBack => _rolledBack;

        public void RegisterCorrelationDataProvider(ICorrelationDataProvider correlationDataProvider)
        {
            _correlationDataProviders.Add(correlationDataProvider);
        }

        public MetaData CorrelationData
        {
            get
            {
                if (!_correlationDataProviders.Any())
                {
                    return MetaData.EmptyInstance;
                }
                var result = new Dictionary<string,object>();
                foreach (var correlationDataProvider in _correlationDataProviders)
                {
                    var extraData = correlationDataProvider.CorrelationDataFor(Message);
                    if (extraData != null)
                    {
                        extraData.ForEach(x => result.Add(x.Key, x.Value));
                    }
                }

                return MetaData.From(result);
            }
        }

        public void OnPrepareCommit(Action<IUnitOfWork> handler)
        {
            AddHandler(Phase.PREPARE_COMMIT, handler);
        }

        public void OnCommit(Action<IUnitOfWork> handler)
        {
            AddHandler(Phase.COMMIT, handler);
        }


        public void AfterCommit(Action<IUnitOfWork> handler)
        {
            AddHandler(Phase.AFTER_COMMIT, handler);
        }

        public void OnRollback(Action<IUnitOfWork> handler)
        {
            AddHandler(Phase.ROLLBACK, handler);
        }

        public void OnCleanup(Action<IUnitOfWork> handler)
        {
            AddHandler(Phase.CLEANUP, handler);
        }

        public Phase Phase 
        {
            get => _phase;
            protected set => _phase = value;
        }

        protected void ChangePhase(params Phase[] phases)
        {
            foreach (var phase in phases)
            {
                Phase = phase;
                NotifyHandlers(phase);
            }
        }

        protected abstract void NotifyHandlers(Phase phase);
        protected abstract void AddHandler(Phase phase, Action<IUnitOfWork> handler);
        protected abstract void SetRollbackCause(Exception cause);
        
        public abstract IMessage Message { get; }


        public abstract IUnitOfWork TransformMessage(Func<IMessage, IMessage> transformOperation);


        public abstract R ExecuteWithResult<R>(Func<R> task, RollbackConfigurationType rollbackConfiguration);

        public abstract Task<object> ExecutionResult { get; set; }
    }
}