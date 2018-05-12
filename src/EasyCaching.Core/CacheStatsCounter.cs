namespace EasyCaching.Core
{
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Cache stats counter.
    /// </summary>
    public class CacheStatsCounter
    {
        private long[] _counters = new long[2];
    
        public void Increment(StatsType statsType)
        {
            Interlocked.Increment(ref _counters[(int)statsType]);
        }            

        public long Get(StatsType statsType)
        {
            return Interlocked.Read(ref _counters[(int)statsType]);
        }
    }

    public enum StatsType
    {
        Hit = 0,

        Missed = 1,
    }

    public class CacheStats
    {
        private readonly ConcurrentDictionary<string, CacheStatsCounter> _counters;

        private const string DEFAULT_KEY = "easycahing_catche_stats";


        public CacheStats()
        {
            _counters = new ConcurrentDictionary<string, CacheStatsCounter>();
        }

        public void OnHit()
        {
            GetCounter().Increment(StatsType.Hit);
        }

        public void OnMiss()
        {
            GetCounter().Increment(StatsType.Missed);
        }

        public long GetStatistic(StatsType statsType)
        {
            return GetCounter().Get(statsType);
        }

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
