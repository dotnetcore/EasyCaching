namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    /// <summary>
    /// Redis bus service collection extensions.
    /// </summary>
    public static class RedisBusServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default redis bus.
        /// </summary>
        /// <returns>The default redis bus.</returns>
        /// <param name="services">Services.</param>
        /// <param name="optionsAction">Options action.</param>
        public static IServiceCollection AddDefaultRedisBus(this IServiceCollection services, Action<RedisBusOptions> optionsAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionsAction, nameof(optionsAction));

            var options = new RedisBusOptions();
            optionsAction?.Invoke(options);
            services.AddSingleton(options);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IEasyCachingBus, DefaultRedisBus>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisSubscriberProvider, RedisSubscriberProvider>();
            //services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }
    }
}
