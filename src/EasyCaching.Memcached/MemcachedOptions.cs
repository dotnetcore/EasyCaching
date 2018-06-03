namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;

    public class MemcachedOptions : BaseProviderOptions
    {
        public MemcachedOptions()
        {
            this.CachingProviderType = CachingProviderType.Memcached;
        }

        public EasyCachingMemcachedClientOptions DBConfig { get; set; } = new EasyCachingMemcachedClientOptions();
    }
}
