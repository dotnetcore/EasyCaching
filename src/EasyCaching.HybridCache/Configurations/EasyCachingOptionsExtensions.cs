namespace Microsoft.Extensions.DependencyInjection
{ 
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.HybridCache;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the hybrid (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">The name of this hybrid provider instance.</param>
        public static EasyCachingOptions UseHybrid(
            this EasyCachingOptions options
            , Action<HybridCachingOptions> configure
            , string name = EasyCachingConstValue.DefaultHybridName
            )
        {
            options.RegisterExtension(new HybridCacheOptionsExtension(name, configure));

            return options;
        }

        /// <summary>
        /// Uses the hybrid (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The Configuraion</param>
        /// <param name="name">The name of this hybrid provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseHybrid(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultHybridName
            , string sectionName = EasyCachingConstValue.HybridSection
            )
        {
            var config = configuration.GetSection(sectionName);
            var hybridOptions = new HybridCachingOptions();
            config.Bind(hybridOptions);

            void configure(HybridCachingOptions x)
            {
                x.EnableLogging = hybridOptions.EnableLogging;
                x.BusRetryCount = hybridOptions.BusRetryCount;
                x.DefaultExpirationForTtlFailed = hybridOptions.DefaultExpirationForTtlFailed;
                x.DistributedCacheProviderName = hybridOptions.DistributedCacheProviderName;
                x.LocalCacheProviderName = hybridOptions.LocalCacheProviderName;
                x.TopicName = hybridOptions.TopicName;
            }
            return options.UseHybrid(configure, name);
        }
    }
}
