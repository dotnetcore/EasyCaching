using AsyncKeyedLock;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLock : IDistributedLock
    {
        private static readonly AsyncKeyedLocker<string> _locker = new AsyncKeyedLocker<string>(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        public string Key { get; }

        private readonly Lock _syncObj = LockFactory.Create();

        public MemoryLock(string key) => Key = key;

        private AsyncKeyedLockReleaser<string> _releaser;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AsyncKeyedLockReleaser<string> GetOrCreate()
        {
            if (Volatile.Read(ref _releaser) != null) throw new DistributedLockException();

            lock (_syncObj)
            {
                if (Volatile.Read(ref _releaser) != null) throw new DistributedLockException();

                var releaser = _locker.GetOrAdd(Key);

                Volatile.Write(ref _releaser, releaser);

                return releaser;
            }
        }

        #region Dispose
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async ValueTask DisposeAsync()
        {
            await ReleaseAsync();

            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing) => Release();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~MemoryLock() => Dispose(false);

        #endregion Dispose

        #region Release
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalRelease()
        {
            var semaphore = Interlocked.Exchange(ref _releaser, null);

            if (semaphore == null) return;

            semaphore.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Release() => InternalRelease();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ValueTask ReleaseAsync()
        {
            InternalRelease();

            return default;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LockFail()
        {
            var semaphore = Interlocked.Exchange(ref _releaser, null);

            if (semaphore == null) return;

            new AsyncKeyedLockTimeoutReleaser<string>(false, semaphore).Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool Lock(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var semaphore = GetOrCreate();

            bool locked = false;
            try
            {
                locked = semaphore.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            }
            finally
            {
                if (!locked) LockFail();
            }

            return locked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async ValueTask<bool> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var semaphore = GetOrCreate();

            bool locked = false;
            try
            {
                locked = await semaphore.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (!locked) LockFail();
            }

            return locked;
        }
    }
}
