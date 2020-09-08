namespace EasyCaching.Core
{    
    /// <summary>
    /// Caching provider type.
    /// </summary>
    public enum CachingProviderType
    {
        InMemory,
        Memcached,
        Redis,
        SQLite,
        Disk,
        Ext1,
        Ext2,
        LiteDB,
    }
}
