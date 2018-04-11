namespace EasyCaching.Redis
{
    using EasyCaching.Core.Internal;

    public class RedisOptions: BaseProviderOptions
    {
        public RedisOptions()
        {
            this.CachingProviderType = CachingProviderType.Redis;
        }
    }
}
