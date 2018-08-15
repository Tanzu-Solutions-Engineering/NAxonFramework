using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;

namespace NAxonFramework.Common.Lock
{
	public class PessimisticLockFactory : ILockFactory
	{
		private object _lock = new object();
		private static PessimisticLockFactory _instance = new PessimisticLockFactory();

		public static PessimisticLockFactory Instance
		{
			get
			{
				EnsureLogicalThreadIdentiy();
				return _instance;
			}
		}

		private static readonly AsyncLocal<Guid> _logicalThreadIdentity = new AsyncLocal<Guid>();

		private static void EnsureLogicalThreadIdentiy()
		{
			if (_logicalThreadIdentity.Value == Guid.Empty)
				_logicalThreadIdentity.Value = Guid.NewGuid();
		}

		private PessimisticLockFactory()
		{

		}

		private ConcurrentDictionary<string, NamedLock> _locks = new ConcurrentDictionary<string, NamedLock>();

		public async Task<ILock> ObtainLock(string name)
		{
			NamedLock lk = null;
			lock (_lock)
			{
				lk = _locks.GetOrAdd(name, key => new NamedLock(key));
			}

			await lk.Lock();
			var disposable = new DisposableLock(Disposable.Create(() =>
			{
				lk.Release();
				lock (_lock)
				{
					if (lk.Waiters.Count == 0)
					{
						_locks.TryRemove(name, out _);
					}
				}
			}));
			return disposable;
		}

		private static HashSet<Guid> ThreadsWaitingForMyLocks(Guid owner, PessimisticLockFactory locksInUse)
		{
			var waitingThreads = new HashSet<Guid>();

			locksInUse._locks.Values.Where(x => x.CurrentOwner == owner)
				.ToList()
				.ForEach(disposableLock => disposableLock.Waiters
					.Where(x => waitingThreads.Add(x))
					.ForEach(threadId => ThreadsWaitingForMyLocks(threadId, locksInUse).ForEach(x => waitingThreads.Add(x))));

			return waitingThreads;
		}

		private class NamedLock
		{

			public HashSet<Guid> Waiters = new HashSet<System.Guid>();
			public Guid CurrentOwner { get; private set; }
			SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
			string identifier;
			public object SynchronizationRoot { get; } = new object();


			public NamedLock(string identifier)
			{
				this.identifier = identifier;
			}

			public async Task Lock()
			{

				EnsureLogicalThreadIdentiy();
				if (IsOwner)
					throw new LockRecursionException();
				Waiters.Add(CurrentThreadId);
				if (!_lock.Wait(0))
				{

					do
					{
						CheckForDeadlock();
					} while (!await _lock.WaitAsync(100));
				}

				CurrentOwner = _logicalThreadIdentity.Value;
				Waiters.Remove(_logicalThreadIdentity.Value);
			}

			private void CheckForDeadlock()
			{
				if (!IsOwner && IsLocked)
				{
					foreach (var threadId in PessimisticLockFactory.ThreadsWaitingForMyLocks(CurrentThreadId, PessimisticLockFactory.Instance))
					{
						if (CurrentOwner == threadId)
						{
							throw new DeadlockException("An imminent deadlock was detected while attempting to acquire a lock");
						}
					}
				}
			}


			public void Release()
			{
				_lock.Release();

			}

			bool IsHeld => CurrentOwner == Guid.Empty;
			bool IsOwner => CurrentOwner == CurrentThreadId;
			bool IsLocked => _lock.CurrentCount == 0;

			Guid CurrentThreadId
			{
				get
				{
					EnsureLogicalThreadIdentiy();
					return _logicalThreadIdentity.Value;
				}
			}


		}
	}
}