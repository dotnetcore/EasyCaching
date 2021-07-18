using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLock : IDistributedLock
    {
        private static readonly RefCounterPool<string, SemaphoreSlim> SemaphoreSlims
            = new RefCounterPool<string, SemaphoreSlim>();

        public string Key { get; }

        private readonly object _syncObj = new object();

        public MemoryLock(string key) => Key = key;

        private SemaphoreSlim _semaphore;

        private SemaphoreSlim GetOrCreate()
        {
            if (Volatile.Read(ref _semaphore) != null) throw new DistributedLockException();

            lock (_syncObj)
            {
                if (Volatile.Read(ref _semaphore) != null) throw new DistributedLockException();

                var semaphore = SemaphoreSlims.GetOrAdd(Key, _ => new SemaphoreSlim(1, 1));

                Volatile.Write(ref _semaphore, semaphore);

                return semaphore;
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

            SemaphoreSlims.TryRemove(Key)?.Dispose();
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

            SemaphoreSlims.TryRemove(Key)?.Dispose();
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
