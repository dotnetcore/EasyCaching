namespace EasyCaching.Memcached
{
    using EasyCaching.Core.Internal;

    public class MemcachedOptions : BaseProviderOptions
    {
        public MemcachedOptions()
        {
            this.CachingProviderType = CachingProviderType.Memcached;
        }
    }
}
