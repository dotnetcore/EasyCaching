namespace EasyCaching.Core
{
    using System.Threading;

    /// <summary>
    /// Cache stats counter.
    /// </summary>
    public class CacheStatsCounter
    {
        /// <summary>
        /// The hited count.
        /// </summary>
        private long _hitedCount = 0;

        /// <summary>
        /// The missed count.
        /// </summary>
        private long _missedCount = 0;

        /// <summary>
        /// Increments the hited.
        /// </summary>
        public void IncrementHited()
        {
            Interlocked.Increment(ref _hitedCount);
        }

        /// <summary>
        /// Increments the missed.
        /// </summary>
        public void IncrementMissed()
        {
            Interlocked.Increment(ref _missedCount);
        }

        /// <summary>
        /// Gets the hited count.
        /// </summary>
        /// <returns>The hited count.</returns>
        public long GetHitedCount() => _hitedCount;

        /// <summary>
        /// Gets the missed count.
        /// </summary>
        /// <returns>The missed count.</returns>
        public long GetMissedCount() => _missedCount;
    }
}
