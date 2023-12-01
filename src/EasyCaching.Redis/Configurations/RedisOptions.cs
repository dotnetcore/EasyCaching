namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions: BaseProviderOptions
    {
        public RedisOptions()
        {

        }

        public RedisDBOptions DBConfig { get; set; } = new RedisDBOptions();
        
        /// <summary>
        /// Whether or not to use Elastic APM, defaults to false
        /// </summary>
        public bool UseApm { get; set; } = false;
    }
}
