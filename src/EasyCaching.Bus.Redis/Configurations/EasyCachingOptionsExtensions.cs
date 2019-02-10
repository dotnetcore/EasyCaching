namespace EasyCaching.Bus.Redis
{
    using System;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the redis bus.
        /// </summary>
        /// <returns>The redis bus.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        public static EasyCachingOptions WithRedisBus(this EasyCachingOptions options, Action<RedisBusOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new RedisBusOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the redis bus.
        /// </summary>
        /// <returns>The redis bus.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions WithRedisBus(this EasyCachingOptions options, IConfiguration configuration, string sectionName = "redisbus")
        {
            var dbConfig = configuration.GetSection(sectionName);
            var redisOptions = new RedisBusOptions();
            dbConfig.Bind(redisOptions);

            void configure(RedisBusOptions x)
            {
                x.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                x.AllowAdmin = redisOptions.AllowAdmin;
                x.Configuration = redisOptions.Configuration;
                x.ConnectionTimeout = redisOptions.ConnectionTimeout;
                x.Database = redisOptions.Database;
                x.IsSsl = redisOptions.IsSsl;
                x.Password = redisOptions.Password;
                x.SslHost = redisOptions.SslHost;

                foreach (var item in redisOptions.Endpoints) x.Endpoints.Add(item);
            }

            options.RegisterExtension(new RedisBusOptionsExtension(configure));
            return options;
        }
    }
}
