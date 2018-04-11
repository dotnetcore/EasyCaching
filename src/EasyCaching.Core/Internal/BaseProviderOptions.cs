namespace EasyCaching.Core.Internal
{
    using System;

    public class BaseProviderOptions
    {
        public CachingProviderType CachingProviderType { get; set; }

        public int MaxRdSecond { get; set; } = 120;

        public int Order { get; set; } 
    }

    public enum CachingProviderType
    {
        InMemory,
        Memcached,
        Redis,
        SQLite,
        Ext1,
        Ext2
    }
}
