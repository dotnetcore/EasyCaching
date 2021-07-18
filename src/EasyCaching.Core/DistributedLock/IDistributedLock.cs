using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    /// <summary>A distributed lock</summary>
    public interface IDistributedLock : IDisposable, IAsyncDisposable
    {
        /// <summary>Lock the resource</summary>
        /// <param name="millisecondsTimeout">The max wait timeout for the lock</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
        /// <returns>The locked resource, if succeed, else return null</returns>
        bool Lock(int millisecondsTimeout, CancellationToken cancellationToken = default);

        /// <summary>Lock the resource</summary>
        /// <param name="millisecondsTimeout">The max wait timeout for the lock</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The locked resource, if succeed, else return null.</returns>
        ValueTask<bool> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken = default);

        /// <summary>Release the resource</summary>
        void Release();

        /// <summary>Release the resource</summary>
        ValueTask ReleaseAsync();
    }
}
