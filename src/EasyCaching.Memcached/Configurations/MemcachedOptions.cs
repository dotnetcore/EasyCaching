namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class MemcachedOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public EasyCachingMemcachedClientOptions DBConfig { get; set; } = new EasyCachingMemcachedClientOptions();
    }
}
