namespace EasyCaching.Redis
{
    using Core;
    using EasyCaching.Core.Configurations;

    public class RedisOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public string ConnectionString { get; set; }
    }
}