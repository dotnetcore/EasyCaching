using System;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.Etcd;
using Microsoft.Extensions.Configuration;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class EtcdCachingOptionsExtensions
{
        /// <summary>
        /// Uses the Etcd provider (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseEtcd(
            this EasyCachingOptions options,
            Action<EtcdCachingOptions> configure,
            string name = EasyCachingConstValue.DefaultEtcdName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new EtcdOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the Etcd provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseEtcd(
            this EasyCachingOptions options, 
            IConfiguration configuration, 
            string name = EasyCachingConstValue.DefaultEtcdName, 
            string sectionName = EasyCachingConstValue.EtcdSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var EtcdOptions = new EtcdCachingOptions();
            dbConfig.Bind(EtcdOptions);

            void Configure(EtcdCachingOptions x)
            {
                x.EnableLogging = EtcdOptions.EnableLogging;
                x.MaxRdSecond = EtcdOptions.MaxRdSecond;             
                x.LockMs = EtcdOptions.LockMs;
                x.SleepMs = EtcdOptions.SleepMs;
                x.SerializerName = EtcdOptions.SerializerName;
                x.CacheNulls = EtcdOptions.CacheNulls;
                x.Address = EtcdOptions.Address;
                x.UserName = EtcdOptions.UserName;
                x.Password = EtcdOptions.Password;
                x.Timeout= EtcdOptions.Timeout;
            }

            options.RegisterExtension(new EtcdOptionsExtension(name, Configure));
            return options;
        }
}