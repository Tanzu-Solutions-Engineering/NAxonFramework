using System;
using System.Threading;
using NAxonFramework.Common.Lock;

namespace NAxonFramework.CommandHandling.Model
{
    public class LockAwareAggregate<AR, A> : IAggregate<AR> where A : IAggregate<AR>
    {
        private readonly ILock _lock;

        public LockAwareAggregate(A wrappedAggregate, ILock @lock)
        {
            _lock = @lock;
            WrappedAggregate = wrappedAggregate;
        }

        public string Type => WrappedAggregate.Type;
        public object Identifier => WrappedAggregate.Identifier;
        public long? Version => WrappedAggregate.Version;
  
        public object Handle(ICommandMessage commandMessage)
        {
            return WrappedAggregate.Handle(commandMessage);
        }

        public R Invoke<R>(Func<AR, R> invocation)
        {
            return WrappedAggregate.Invoke(invocation);
        }

        public void Execute(Action<AR> invocation)
        {
            WrappedAggregate.Execute(invocation);
        }


        public bool IsDeleted => WrappedAggregate.IsDeleted;
        public Type RootType => WrappedAggregate.RootType;
        public A WrappedAggregate { get; }
        public bool IsLockHeld => _lock.IsHeld;
    }
}