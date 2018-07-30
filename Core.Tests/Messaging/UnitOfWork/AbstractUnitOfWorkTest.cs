using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Transactions;
using CommonServiceLocator;
using Core.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.UnitOfWork;
using NSubstitute;
using Xunit;

namespace Core.Tests.Messaging.UnitOfWork
{
    public class AbstractUnitOfWorkTest
    {
        private List<PhaseTransition> _phaseTransitoins;
        private IUnitOfWork _subject;

        public AbstractUnitOfWorkTest()
        {
            var serviceLocator = Substitute.For<IServiceLocator>();
            var logger = Substitute.For<ILogger<AbstractUnitOfWork>>();
            serviceLocator.GetInstance<ILogger<AbstractUnitOfWork>>().Returns(logger);
            
            CommonServiceLocator.ServiceLocator.SetLocatorProvider(() => serviceLocator);
            while (CurrentUnitOfWork.IsStarted)
            {
                CurrentUnitOfWork.Get().Rollback();
            }

            _subject = Substitute.ForPartsOf<DefaultUnitOfWork>(new GenericEventMessage<string>("Input 1"));
            //_subject.ToString().Returns("unitOfWork");
            _phaseTransitoins = new List<PhaseTransition>();
            RegisterListeners(_subject);
        }

        private void RegisterListeners(IUnitOfWork unitOfWork)
        {
            unitOfWork.OnPrepareCommit(u => _phaseTransitoins.Add(new PhaseTransition(u, Phase.PREPARE_COMMIT)));
            unitOfWork.OnCommit(u => _phaseTransitoins.Add(new PhaseTransition(u, Phase.COMMIT)));
            unitOfWork.AfterCommit(u => _phaseTransitoins.Add(new PhaseTransition(u, Phase.AFTER_COMMIT)));
            unitOfWork.OnRollback(u => _phaseTransitoins.Add(new PhaseTransition(u, Phase.ROLLBACK)));
            unitOfWork.OnCleanup(u => _phaseTransitoins.Add(new PhaseTransition(u, Phase.CLEANUP)));
        }
        
        [Fact]
        public void TestHandlersForCurrentPhaseAreExecuted() {
            bool prepareCommit = false;
            bool commit = false;
            bool afterCommit = false;
            bool cleanup = false;
            _subject.OnPrepareCommit(u => _subject.OnPrepareCommit(i => prepareCommit = true));
            _subject.OnCommit(u => _subject.OnCommit(i => commit = true));
            _subject.AfterCommit(u => _subject.AfterCommit(i => afterCommit = true));
            _subject.OnCleanup(u => _subject.OnCleanup(i => cleanup = true));
            
            _subject.Start();
            _subject.Commit();

            prepareCommit.Should().BeTrue();
            commit.Should().BeTrue();
            afterCommit.Should().BeTrue();
            cleanup.Should().BeTrue();

        }

        [Fact]
        public void TestExecuteTask()
        {
            bool taskRun = false;
            bool commited = false;
            _subject.OnCommit(_ => commited = true);
            
            _subject.Execute(() => taskRun = true);

            commited.Should().BeTrue();
            taskRun.Should().BeTrue();
            _subject.IsActive().Should().BeFalse();
        }

        [Fact]
        public void TestExecuteFailingTask()
        {
            bool rollback = false;
            _subject.OnRollback(_ => rollback = true);
            
            Assert.Throws<MockException>(() => _subject.Execute(() => throw new MockException()));
            _subject.ExecutionResult.Exception.Should().NotBeNull();
            _subject.ExecutionResult.Exception.GetBaseException().Should().BeOfType<MockException>();
            rollback.Should().BeTrue();
        }

        [Fact]
        public void TestExecuteWithResult()
        {
            var taskResult = new object();
            bool commited = false;
            _subject.OnCommit(_ => commited = true);
            
            var result = _subject.ExecuteWithResult(() => taskResult);

            commited.Should().BeTrue();
            _subject.IsActive().Should().BeFalse();
            _subject.ExecutionResult.Should().NotBeNull();
            result.Should().BeSameAs(taskResult);
            _subject.ExecutionResult.Result.Should().BeSameAs(taskResult);

        }

        [Fact]
        public void TestAttachedTransactionCommittedOnUnitOfWorkCommit()
        {
            var enlistment = Substitute.For<IEnlistmentNotification>();
            enlistment.Prepare(Arg.Do<PreparingEnlistment>(arg => arg.Prepared()));
            enlistment.Commit(Arg.Do<Enlistment>(e => e.Done()));
            
            _subject.Start();

            Transaction.Current.Should().NotBeNull();
            Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.None);

            _subject.Commit();

            enlistment.Received().Prepare(Arg.Any<PreparingEnlistment>());
            enlistment.Received().Commit(Arg.Any<Enlistment>());
        }
        [Fact]
        public void TestAttachedTransactionRolledBackOnUnitOfWorkRollBack()
        {
            var enlistment = Substitute.For<IEnlistmentNotification>();
            enlistment.Prepare(Arg.Do<PreparingEnlistment>(arg => arg.Prepared()));
            enlistment.Rollback(Arg.Do<Enlistment>(e => e.Done()));
            _subject.Start();

            Transaction.Current.Should().NotBeNull();
            Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.None);

            _subject.Rollback();

            enlistment.DidNotReceive().Prepare(Arg.Any<PreparingEnlistment>());
            enlistment.DidNotReceive().Commit(Arg.Any<Enlistment>());
            enlistment.Received().Rollback(Arg.Any<Enlistment>());
        }

        [Fact]
        public void UnitOfWorkIsRolledBackWhenTransactionFailsToStart()
        {
            var enlistment = Substitute.For<IEnlistmentNotification>();
            enlistment.Prepare(Arg.Do<PreparingEnlistment>(arg => arg.Prepared()));
            enlistment.Rollback(Arg.Do<Enlistment>(e => e.Done()));
        }

        private class PhaseTransition {
            protected bool Equals(PhaseTransition other)
            {
                return _phase == other._phase && Equals(_unitOfWork, other._unitOfWork);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int) _phase * 397) ^ (_unitOfWork != null ? _unitOfWork.GetHashCode() : 0);
                }
            }

            private readonly Phase _phase;
            private readonly IUnitOfWork _unitOfWork;

            public PhaseTransition(IUnitOfWork unitOfWork, Phase phase)
            {
                _unitOfWork = unitOfWork;
                _phase = phase;
            }

            public override bool Equals(object o)
            {
                if (ReferenceEquals(null, o)) return false;
                if (ReferenceEquals(this, o)) return true;
                if (o.GetType() != this.GetType()) return false;
                return Equals((PhaseTransition) o);
            }

            public override string ToString() => $"{_unitOfWork} {_phase}";
        }
    }
}