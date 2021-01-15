namespace EasyCaching.Redis
{
    using Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions : BaseProviderOptionsWithDecorator<IRedisAndEasyCachingProvider>
    {
        public RedisDBOptions DBConfig { get; set; } = new RedisDBOptions();
    }
}