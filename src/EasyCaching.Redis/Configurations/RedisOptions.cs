namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions: BaseProviderOptions
    {
        public RedisOptions()
        {
            this.CachingProviderType = CachingProviderType.Redis;
        }

        public RedisDBOptions DBConfig { get; set; } = new RedisDBOptions();
    }
}
