namespace Microsoft.Extensions.Caching.EasyCachingLib
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;

    public static class EasyCachingRedisCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyCachingRedisCache(this IServiceCollection services)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.Add(ServiceDescriptor.Singleton<IDistributedCache, EasyCachingRedisCache>());

            return services;
        }
    }
}
