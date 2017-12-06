namespace EasyCaching.Core
{
    /// <summary>
    /// EasyCaching type.
    /// </summary>
    public enum EasyCachingType
    {
        /// <summary>
        /// The Memory Cache (Local).
        /// </summary>
        Memory = 0,

        /// <summary>
        /// The SQLite Cache (Local).
        /// </summary>
        SQLite = 1,

        /// <summary>
        /// The Redis Cache (Remote).
        /// </summary>
        Redis = 2,

        /// <summary>
        /// The Memcached (Remote).
        /// </summary>
        Memcached = 3,
    }
}
