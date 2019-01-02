using System;

namespace NAxonFramework.Common.Lock
{
    public class DisposableLock : ILock
    {
        private readonly IDisposable _lock;

        public DisposableLock(IDisposable @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            IsHeld = false;
            _lock.Dispose();
        }

        public bool IsHeld { get; private set; } = true;
    }
}