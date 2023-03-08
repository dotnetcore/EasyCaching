namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Bus.Zookeeper;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions 
    {
        /// <summary>
        /// Withs the Zookeeper bus (specify the config via hard code).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static EasyCachingOptions WithZookeeeperBus(
            this EasyCachingOptions options
            , Action<ZkBusOptions> configure
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));
            options.RegisterExtension(new ZookeeperOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the zookeeper bus (read config from configuration file).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        /// <returns></returns>
        public static EasyCachingOptions WithZookeeeperBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string sectionName = EasyCachingConstValue.ZookeeperBusSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var zkOptions = new ZkBusOptions();
            dbConfig.Bind(zkOptions);

            void configure(ZkBusOptions x)
            {
                x.ConnectionString = zkOptions.ConnectionString;
                x.SessionTimeout = zkOptions.SessionTimeout;
                x.OperatingTimeout = zkOptions.OperatingTimeout;
                x.ConnectionTimeout = zkOptions.ConnectionTimeout;
                x.Digest = zkOptions.Digest;
                x.BaseRoutePath = zkOptions.BaseRoutePath;
                x.ReadOnly = zkOptions.ReadOnly;
                x.BaseRoutePath = zkOptions.BaseRoutePath;
                x.LogToFile = zkOptions.LogToFile;
            }

            options.RegisterExtension(new ZookeeperOptionsExtension(configure));
            return options;
        }
    }
}
