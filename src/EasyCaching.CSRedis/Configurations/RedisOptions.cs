namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions : BaseProviderOptions
    {
        public RedisOptions()
        {
            this.CachingProviderType = CachingProviderType.Redis;
        }

        /// <summary>
        /// Gets or sets the DBC onfig.
        /// </summary>
        /// <value>The DBC onfig.</value>
        public CSRedisDBOptions DBConfig { get; set; } = new CSRedisDBOptions();
    }
}
