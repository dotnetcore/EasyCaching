namespace EasyCaching.CSRedis
{
    using Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions : BaseProviderOptionsWithDecorator<IRedisAndEasyCachingProvider>
    {
        /// <summary>
        /// Gets or sets the DBC onfig.
        /// </summary>
        /// <value>The DBC onfig.</value>
        public CSRedisDBOptions DBConfig { get; set; } = new CSRedisDBOptions();
    }
}
