using System;
using System.Collections.Generic;
using System.Threading;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public static class CurrentUnitOfWork
    {
        private static readonly AsyncLocal<Stack<IUnitOfWork>> Current = new AsyncLocal<Stack<IUnitOfWork>>();
        public static bool IsStarted => Current.Value != null && !Current.Value.IsEmpty();

        public static bool IfStarted(Action<IUnitOfWork> consumer)
        {
            if (IsStarted)
            {
                consumer(Get());
                return true;
            }

            return false;
        }

        public static Optional<T> Map<T>(Func<IUnitOfWork, T> function) => IsStarted ? Optional<T>.Of(function(Get())) : Optional<T>.Empty;

        public static IUnitOfWork Get()
        {
            
                if (IsEmpty)
                {
                    throw new InvalidOperationException("No UnitOfWork is currently started for this thread.");
                }

                return Current.Value.Peek();
        }

        public static bool IsEmpty => Current.Value == null || Current.Value.IsEmpty();
        public static void Commit() => Get().Commit();

        public static void Set(IUnitOfWork unitOfWork)
        {
            if(Current.Value == null)
                Current.Value = new Stack<IUnitOfWork>();
            Current.Value.Push(unitOfWork);
        }

        public static void Clear(IUnitOfWork unitOfWork)
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException("Could not clear this UnitOfWork. There is no UnitOfWork active.");
            }

            if (Current.Value.Peek() == unitOfWork)
            {
                Current.Value.Pop();
                if (Current.Value.IsEmpty())
                {
                    Current.Value = null;
                }
                else
                {
                    throw new InvalidOperationException("Could not clear this UnitOfWork. It is not the active one.");
                }
            }
        }

        public static MetaData CorrelationData => CurrentUnitOfWork.Map(x => x.CorrelationData).OrElse(MetaData.EmptyInstance);
    }
}