using System;
using System.Threading;

namespace NAxonFramework.Serialization
{
    public class CachingSupplier<T> 
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private T _value;
        private readonly Func<T> _delegate;
        public CachingSupplier()
        {
        }

        public static CachingSupplier<T> Of(T  value) => new CachingSupplier<T>(value);

        public static CachingSupplier<T> Of(Func<T>  value) => new CachingSupplier<T>(value);

        private CachingSupplier(Func<T> value)
        {
            _delegate = value;
        }

        private CachingSupplier(T value)
        {
            _value = value;
            _delegate = () => _value;
        }

        public T Get()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_value == null)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _value = _delegate();
                    }
                    finally 
                    {
                        _lock.ExitWriteLock();
                    }
                    
                }

                return _value;
            }
            finally 
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }
}