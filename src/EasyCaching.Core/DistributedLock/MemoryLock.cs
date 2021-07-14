using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLock : IDistributedLock
    {
        private static readonly RefCounterPool<string, SemaphoreSlim> SemaphoreSlims
            = new RefCounterPool<string, SemaphoreSlim>();

        private readonly string _key;
        private readonly object _syncObj = new object();

        public MemoryLock(string key) => _key = key;

        private volatile SemaphoreSlim _semaphore;

        private void GetOrCreate()
        {
            if (_semaphore != null) throw new DistributedLockException();

            lock (_syncObj)
            {
                if (_semaphore != null) throw new DistributedLockException();

                _semaphore = SemaphoreSlims.GetOrAdd(_key, key => new SemaphoreSlim(1, 1));
            }
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await ReleaseAsync().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => Release();

        ~MemoryLock() => Dispose(false);

        #endregion Dispose

        private void InternalRelease()
        {
            var semaphore = Interlocked.Exchange(ref _semaphore, null);

            if (semaphore == null) return;

            semaphore.Release();

            SemaphoreSlims.TryRemove(_key)?.Dispose();
        }

        public virtual void Release() => InternalRelease();

        public virtual ValueTask ReleaseAsync()
        {
            InternalRelease();

            return default;
        }

        public virtual bool Lock(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            GetOrCreate();

            var locked = false;
            try
            {
                if (_semaphore != null) locked = _semaphore.Wait(millisecondsTimeout, cancellationToken);
            }
            finally
            {
                if (!locked) _semaphore = null;
            }

            return locked;
        }

        public virtual async ValueTask<bool> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            GetOrCreate();

            var locked = false;
            try
            {
                if (_semaphore != null) locked = await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (!locked) _semaphore = null;
            }

            return locked;
        }
    }
}
