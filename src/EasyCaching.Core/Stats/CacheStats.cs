namespace EasyCaching.Core
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Cache stats.
    /// </summary>
    public class CacheStats
    {
        /// <summary>
        /// The counters.
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheStatsCounter> _counters;

        /// <summary>
        /// The default key.
        /// </summary>
        private const string DEFAULT_KEY = "easycahing_catche_stats";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Core.CacheStats"/> class.
        /// </summary>
        public CacheStats()
        {
            _counters = new ConcurrentDictionary<string, CacheStatsCounter>();
        }

        /// <summary>
        /// Ons the hit.
        /// </summary>
        public void OnHit()
        {
            GetCounter().Increment(StatsType.Hit);
        }

        /// <summary>
        /// Ons the miss.
        /// </summary>
        public void OnMiss()
        {
            GetCounter().Increment(StatsType.Missed);
        }

        /// <summary>
        /// Gets the statistic.
        /// </summary>
        /// <returns>The statistic.</returns>
        /// <param name="statsType">Stats type.</param>
        public long GetStatistic(StatsType statsType)
        {
            return GetCounter().Get(statsType);
        }

        /// <summary>
        /// Gets the counter.
        /// </summary>
        /// <returns>The counter.</returns>
        private CacheStatsCounter GetCounter()
        {
            if (!_counters.TryGetValue(DEFAULT_KEY, out CacheStatsCounter counter))
            {
                counter = new CacheStatsCounter();
                if (_counters.TryAdd(DEFAULT_KEY, counter))
                {
                    return counter;
                }

                return GetCounter();
            }

            return counter;
        }
    }
}
