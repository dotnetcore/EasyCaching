namespace EasyCaching.Core
{
    using System.Threading;

    /// <summary>
    /// Cache stats counter.
    /// </summary>
    public class CacheStatsCounter
    {
        /// <summary>
        /// The counters.
        /// </summary>
        private long[] _counters = new long[2];
    
        /// <summary>
        /// Increment the specified statsType.
        /// </summary>
        /// <param name="statsType">Stats type.</param>
        public void Increment(StatsType statsType)
        {
            Interlocked.Increment(ref _counters[(int)statsType]);
        }            

        /// <summary>
        /// Get the specified statsType.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="statsType">Stats type.</param>
        public long Get(StatsType statsType)
        {
            return Interlocked.Read(ref _counters[(int)statsType]);
        }
    }      
}
