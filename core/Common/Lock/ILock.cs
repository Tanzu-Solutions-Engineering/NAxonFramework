using System;

namespace NAxonFramework.Common.Lock
{
    public interface ILock : IDisposable
    {
        bool IsHeld { get; }
    }
}