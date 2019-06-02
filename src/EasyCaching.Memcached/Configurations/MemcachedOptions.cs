namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class MemcachedOptions : BaseProviderOptions
    {
        public MemcachedOptions()
        {

        }

        public EasyCachingMemcachedClientOptions DBConfig { get; set; } = new EasyCachingMemcachedClientOptions();
    }
}
