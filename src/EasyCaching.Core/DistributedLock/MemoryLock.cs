using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLock : IDistributedLock
    {
        private readonly RefCounterPool<string, SemaphoreSlim> _semaphoreSlims
            = new RefCounterPool<string, SemaphoreSlim>();//MemoryLock must be singleton.

        private readonly string _key;
        private readonly object _syncObj = new object();

        public MemoryLock(string key) => _key = key;

        private volatile SemaphoreSlim _semaphore;

        private SemaphoreSlim GetOrCreate()
        {
            if (_semaphore != null) throw new DistributedLockException();

            lock (_syncObj)
            {
                if (_semaphore != null) throw new DistributedLockException();

                _semaphore = _semaphoreSlims.GetOrAdd(_key, _ => new SemaphoreSlim(1, 1));
            }

            return _semaphore;
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await ReleaseAsync();

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => Release();

        ~MemoryLock() => Dispose(false);

        #endregion Dispose

        #region Release
        private void InternalRelease()
        {
            var semaphore = Interlocked.Exchange(ref _semaphore, null);

            if (semaphore == null) return;

            semaphore.Release();

            _semaphoreSlims.TryRemove(_key)?.Dispose();
        }

        public virtual void Release() => InternalRelease();

        public virtual ValueTask ReleaseAsync()
        {
            InternalRelease();

            return default;
        }
        #endregion

        private void LockFail()
        {
            var semaphore = Interlocked.Exchange(ref _semaphore, null);

            if (semaphore == null) return;

            _semaphoreSlims.TryRemove(_key)?.Dispose();
        }

        public virtual bool Lock(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var semaphore = GetOrCreate();

            var locked = false;
            try
            {
                locked = semaphore.Wait(millisecondsTimeout, cancellationToken);
            }
            finally
            {
                if (!locked) LockFail();
            }

            return locked;
        }

        public virtual async ValueTask<bool> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var semaphore = GetOrCreate();

            var locked = false;
            try
            {
                locked = await semaphore.WaitAsync(millisecondsTimeout, cancellationToken);
            }
            finally
            {
                if (!locked) LockFail();
            }

            return locked;
        }
    }
}
