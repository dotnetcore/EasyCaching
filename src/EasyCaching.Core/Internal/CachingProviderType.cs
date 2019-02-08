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
        SQLServer,
        Ext2
    }
}
