using System;
using EasyCaching.Core;
using EasyCaching.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCaching.Sync.Redis
{
    public static class RedisBusServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRedisCache(this IServiceCollection services, Action<RedisBusOptions> optionsAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionsAction, nameof(optionsAction));

            services.AddOptions();
            services.Configure(optionsAction);

            //services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisSubscriberProvider, RedisSubscriberProvider>();
            //services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }
    }
}
