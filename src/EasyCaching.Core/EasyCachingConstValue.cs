namespace EasyCaching.Core
{
    /// <summary>
    /// EasyCaching const value.
    /// </summary>
    public class EasyCachingConstValue
    {
        /// <summary>
        /// The config section.
        /// </summary>
        public const string ConfigSection = "easycaching";

        /// <summary>
        /// The redis section.
        /// </summary>
        public const string RedisSection = "easycaching:redis";

        /// <summary>
        /// The memcached section.
        /// </summary>
        public const string MemcachedSection = "easycaching:memcached";

        /// <summary>
        /// The SQLite section.
        /// </summary>
        public const string SQLiteSection = "easycaching:sqlite";

        /// <summary>
        /// The in-memory section.
        /// </summary>
        public const string InMemorySection = "easycaching:inmemory";

        public const string DefaultName = "Default";
    }
}
